# Iguina LowRes Theme

A low-resolution UI theme for the Iguina UI library, ported from
GeonBit.UI 4.3's "LowRes" theme.

## What this is

`Iguina.LowResTheme/` is a self-contained Iguina theme: PNG textures
+ Iguina-format JSON stylesheets + a `system_style.json` entry point.
It targets the same visual look-and-feel as GeonBit.UI's LowRes theme
while keeping Iguina's architecture: each per-state texture is loaded
by its own id (no atlas packing), stylesheets reference rectangles
into those individual PNGs, the renderer's existing texture cache
deduplicates per-id loads.

Drop it into a launcher's output directory (or wherever your
`UISystem(systemStylePath, ...)` constructor loads from) and Iguina
will pick up the look. See `Iguina.Demo/Assets/DefaultTheme/` for an
example of Iguina's stylesheet format if you want to customize.

## Asset credits

The PNG textures are copied from GeonBit.UI 4.3.0.4's LowRes theme,
which credits Michele Bucelli ("Buch") on OpenGameArt:

- http://opengameart.org/content/golden-ui
- http://opengameart.org/content/roguelikerpg-icons
- http://opengameart.org/content/roguelikerpg-items
- http://opengameart.org/content/arabian-icons
- http://opengameart.org/content/2d-static-spritesicons
- http://opengameart.org/content/30-ability-icons
- http://opengameart.org/content/whispers-of-avalon-item-icons

GeonBit.UI is MIT-licensed; the source assets on OpenGameArt are
CC0/CC-BY-licensed; the combined theme ships under MIT.

## Differences from GeonBit's LowRes

- **No atlas packing.** GeonBit ships per-state PNGs as individual
  files (`button_default.png`, `button_default_hover.png`, etc.).
  This theme keeps that layout, with each Iguina stylesheet state
  setting its own `TextureId`. Iguina's `DrawUtils.Draw` honours
  per-stylesheet `TextureId` (falling back to `SystemStyleSheet.DefaultTexture`)
  so no atlas packing is needed.
- **9-slice frame widths** come from the original `_md.xml` files
  (fraction-of-texture-size), converted to pixel `FrameWidth` values
  in each JSON. E.g. `button_default.png` is 32×16 with `FrameWidth=0.2,
  FrameHeight=0.35` in GeonBit → `FrameWidth: {X: 6, Y: 6}` here.
- **No XML, no MGCB.** GeonBit's theme XML descriptors are replaced
  by Iguina JSON; the .fx shader is replaced by the renderer's CPU
  grayscale fallback for the "disabled" state; the .spritefont is
  replaced by FontStashSharp's runtime TTF rendering.

## Coverage

Stylesheets included: panel, button (default + alternative + fancy),
panel_simple, panel_fancy, panel_golden, panel_tabs_button, checkbox,
radio_button, text_input, numeric_input, numeric_input_button,
paragraph, title, horizontal_line, vertical_line, list_panel, list_item,
dropdown_icon, slider_horizontal, slider_handle, slider_vertical,
slider_vertical_handle, color_slider_horizontal,
color_slider_horizontal_handle, scrollbar_vertical,
scrollbar_vertical_handle, progress_bar_horizontal,
progress_bar_horizontal_fill, progress_bar_vertical,
progress_bar_vertical_fill, message_box_backdrop, color_picker,
color_picker_handle, color_buttons_panel, color_buttons_button.

Textures: top-level lowres widgets (38 PNGs) plus the `Icons/`
subfolder (~50 item icons — Apple, Sword, Armor, etc.) for use with
the `Icon` / `Image` entities.

Fonts: `Pixel_Regular.fnt` + `Pixel_Regular_0.png` (Pixel UniCode,
bitmap-rasterized at 30px — the GeonBit lowres font, pixel-perfect).

All Iguina-supplied widget slots (per
`Iguina/UISystem.cs::_DefaultStylesheets`) are now wired. The newer
Iguina-only widgets (vertical sliders, vertical line, color slider,
color buttons, numeric input, vertical progress bar) had no
counterpart in GeonBit.UI's LowRes theme; their textures here are
either generated (HSV gradient for the color picker / hue strip for
the color slider) or built by rotating the matching horizontal
LowRes texture 90° (sliders, progress bar, horizontal line).
