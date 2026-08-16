"""WPF 像素风图标生成器：Bad Luck Picker 课堂随机抽取器图标。

用法：
    python tools/generate_ico.py --out 优化界面.ico [--preview preview.png]

设计规则见 wpf-pixel-ui 技能的 references/icon-design.md：
- 16x16 画布，1px 深棕外框，内部 12x12 作画
- 主体 = 扑克牌（点名卡牌动画）+ 五点骰子（公平随机抽取）+ 琥珀角标
- 牌底深棕条 = 牌堆厚度/硬阴影，呼应插牌动画
- 每个字符代表一种颜色（"." 为透明），颜色取自调色板资源键
"""

import argparse
import struct
import zlib
from pathlib import Path

# 与 App.xaml / references/palette.md 调色板一一对应
PALETTE = {
    ".": (0, 0, 0, 0),            # 透明
    "#": (74, 56, 39, 255),       # BorderDark 外框/硬阴影
    "F": (242, 230, 205, 255),    # PaperBackdrop 纸面
    "P": (239, 226, 198, 255),    # PaperTextureA 点阵纹理
    "L": (255, 249, 236, 255),    # BorderLight 内层高光
    "C": (255, 247, 232, 255),    # CardFill 卡片/骰子白点
    "A": (232, 163, 61, 255),     # AmberBrush 琥珀角标
    "I": (59, 46, 35, 255),       # InkBrush 瞳孔/文字
    "R": (217, 79, 48, 255),      # AccentBrush 暖红骰面
    "D": (122, 46, 28, 255),      # AccentDarkBrush 深红
    "G": (111, 168, 107, 255),    # GreenBrush 浅绿
    "g": (22, 101, 52, 255),      # DeepGreenBrush 深绿
}

BASE = [
    "................",
    ".##############.",
    ".#PFFFFFFFFFAA#.",
    ".#.#########AA#.",
    ".#.#CCCCCCCC#.#.",
    ".#.#C######C#.#.",
    ".#.#C#CRRC#C#.#.",
    ".#.#C#RCCR#C#.#.",
    ".#.#C#RCCR#C#.#.",
    ".#.#C#CRRC#C#.#.",
    ".#.#C######C#.#.",
    ".#.#CCCCCCCC#.#.",
    ".#.##########.#.",
    ".#..###########.",
    ".##############.",
    "................",
]

SIZES = (16, 32, 48, 64)


def png_chunk(tag: bytes, data: bytes) -> bytes:
    return (
        struct.pack(">I", len(data))
        + tag
        + data
        + struct.pack(">I", zlib.crc32(tag + data))
    )


def make_png(pixels: list[list[tuple[int, int, int, int]]], size: int) -> bytes:
    raw = b"".join(
        b"\x00" + b"".join(bytes(pixels[y][x]) for x in range(size))
        for y in range(size)
    )
    ihdr = struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0)
    return (
        b"\x89PNG\r\n\x1a\n"
        + png_chunk(b"IHDR", ihdr)
        + png_chunk(b"IDAT", zlib.compress(raw, 9))
        + png_chunk(b"IEND", b"")
    )


def upscale(size: int) -> list[list[tuple[int, int, int, int]]]:
    return [
        [PALETTE[BASE[y * 16 // size][x * 16 // size]] for x in range(size)]
        for y in range(size)
    ]


def validate_grid() -> list[str]:
    errors: list[str] = []
    if len(BASE) != 16:
        errors.append(f"BASE 必须恰好 16 行，当前 {len(BASE)} 行")
    for i, row in enumerate(BASE):
        if len(row) != 16:
            errors.append(f"第 {i + 1} 行长度为 {len(row)}，应为 16")
        for j, ch in enumerate(row):
            if ch not in PALETTE:
                errors.append(f"第 {i + 1} 行第 {j + 1} 列字符 {ch!r} 未在 PALETTE 中定义")
    return errors


def build_ico() -> bytes:
    images = [(size, make_png(upscale(size), size)) for size in SIZES]
    header = struct.pack("<HHH", 0, 1, len(images))
    offset = 6 + 16 * len(images)
    entries = b""
    payload = b""
    for size, png in images:
        entries += struct.pack("<BBBBHHII", size, size, 0, 0, 1, 32, len(png), offset)
        payload += png
        offset += len(png)
    return header + entries + payload


def main() -> None:
    parser = argparse.ArgumentParser(description="WPF 像素风多尺寸 ICO 生成器")
    parser.add_argument("--out", type=Path, default=Path("app.ico"), help="输出 ICO 路径")
    parser.add_argument("--preview", type=Path, help="可选：输出 512px 最近邻放大预览 PNG")
    args = parser.parse_args()

    errors = validate_grid()
    if errors:
        print("像素网格校验失败：")
        for e in errors:
            print("  -", e)
        raise SystemExit(1)

    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_bytes(build_ico())
    print(f"ICO 已生成：{args.out}（{len(SIZES)} 个尺寸）")

    if args.preview:
        args.preview.parent.mkdir(parents=True, exist_ok=True)
        args.preview.write_bytes(make_png(upscale(512), 512))
        print(f"预览图已生成：{args.preview}")


if __name__ == "__main__":
    main()
