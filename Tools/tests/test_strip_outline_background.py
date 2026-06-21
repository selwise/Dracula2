import importlib.util
import pathlib
import tempfile
import unittest

from PIL import Image


REPO_ROOT = pathlib.Path(__file__).resolve().parents[2]
SCRIPT_PATH = REPO_ROOT / "Tools" / "strip_outline_background.py"


def load_tool():
    assert SCRIPT_PATH.exists(), "expected Tools/strip_outline_background.py to exist"
    spec = importlib.util.spec_from_file_location("strip_outline_background", SCRIPT_PATH)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class StripOutlineBackgroundTests(unittest.TestCase):
    def test_strips_only_border_connected_white_and_grey_pixels(self):
        tool = load_tool()
        image = Image.new("RGBA", (8, 8), (255, 255, 255, 255))
        pixels = image.load()

        for x in range(2, 6):
            pixels[x, 2] = (5, 4, 12, 255)
            pixels[x, 5] = (5, 4, 12, 255)
        for y in range(2, 6):
            pixels[2, y] = (5, 4, 12, 255)
            pixels[5, y] = (5, 4, 12, 255)

        pixels[1, 1] = (190, 190, 196, 255)
        pixels[3, 3] = (255, 255, 255, 255)
        pixels[4, 3] = (177, 174, 193, 255)
        pixels[3, 4] = (70, 20, 30, 255)

        cleaned = tool.strip_border_connected_light_neutral_pixels(image)
        out = cleaned.load()

        self.assertEqual(out[0, 0][3], 0)
        self.assertEqual(out[1, 1][3], 0)
        self.assertEqual(out[2, 2], (5, 4, 12, 255))
        self.assertEqual(out[3, 3], (255, 255, 255, 255))
        self.assertEqual(out[4, 3], (177, 174, 193, 255))
        self.assertEqual(out[3, 4], (70, 20, 30, 255))

    def test_cli_writes_cleaned_png_without_mutating_source(self):
        tool = load_tool()
        image = Image.new("RGBA", (5, 5), (255, 255, 255, 255))
        pixels = image.load()
        for x in range(1, 4):
            pixels[x, 1] = (3, 3, 8, 255)
            pixels[x, 3] = (3, 3, 8, 255)
        for y in range(1, 4):
            pixels[1, y] = (3, 3, 8, 255)
            pixels[3, y] = (3, 3, 8, 255)
        pixels[2, 2] = (210, 210, 210, 255)

        with tempfile.TemporaryDirectory() as tmpdir:
            source = pathlib.Path(tmpdir) / "source.png"
            output = pathlib.Path(tmpdir) / "cleaned.png"
            image.save(source)

            exit_code = tool.main([str(source), str(output)])

            self.assertEqual(exit_code, 0)
            self.assertTrue(output.exists())
            self.assertEqual(Image.open(source).convert("RGBA").load()[0, 0], (255, 255, 255, 255))
            cleaned = Image.open(output).convert("RGBA").load()
            self.assertEqual(cleaned[0, 0][3], 0)
            self.assertEqual(cleaned[1, 1], (3, 3, 8, 255))
            self.assertEqual(cleaned[2, 2], (210, 210, 210, 255))


if __name__ == "__main__":
    unittest.main()
