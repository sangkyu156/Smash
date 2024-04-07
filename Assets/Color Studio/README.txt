****************
* COLOR STUDIO *
* README FILE  *
****************


�� �ڻ��� ����ϴ� ���
---------------------
1) Unity Editor ������ �ֻ��� �޴� Window -> Color Studio -> Palette Manager �Ǵ� Pixel Painter�� �����մϴ�.

2) �� ������ ǥ�õ� ��� �������̽� â�� ��� �ٸ� â�� ��ŷ�ϴ� ���� �����ϴ�.
https://youtu.be/Qe2yHCqWhoU

3) ?�� Ŭ���ϼ���. ���� ��ħ�� ���� Color Studio â�� ��

4) Pixel Painter â�� ����Ͽ� �ȼ� ��Ʈ �����

5) ��Ÿ�ӿ� ������ �����Ϸ��� ���� ��ü�� ��������Ʈ�� "Recolor" ��ũ��Ʈ�� �߰��ϼ���.



��ũ���� ����
-----------------

Color Studio�� UI �������̽��� ���� ������ Ȯ������ ����Ǿ����� �Ϻ� ����� ��ũ������ ����Ͽ� Ȱ���� �� �ֽ��ϴ�.
Ư�� CSPalette Ŭ������ ������ ���� �ν��Ͻ�ȭ�� �� �ֽ��ϴ�.

// �������� �⺻���� �ϴ� ���� ���� �������� �⺻���� ����ϴ� �ȷ�Ʈ�� ����ϴ�.
CSPalette palette = ScriptableObject.CreateInstance<CSPalette>();
palette.ConfigurePalette(ColorScheme.SplitComplementary, Color.red, splitAmount: 0.6f);

// ��ä�� �����
palette.BuildHueColors();

// �ȷ�Ʈ ���� ���� ���
for (int k=0;k<palette.colorsCount;k++) {
    Debug.Log(palette.colors[k]);
}

// �ȷ�Ʈ ���� ����(���� ����)
palette.BuildPaletteColors();
for (int k=0;k<paletteColors.Length;k++) {
    Debug.Log(palette.paletteColors[k]);
}

��Ÿ ������ ���(�޼ҵ�):

- Texture2D ExportLUT(): ���� ���� �ȷ�Ʈ�� ������� LUT�� �����մϴ�.
- Texture2D ExportTexture(): ���� �ȷ�Ʈ�� �����ϴ� �ؽ�ó�� ����ϴ�.
- Color ApplyLUT(Color, LUT): �־��� LUT�� ������� ������ ��ȯ�մϴ�.
- Color GetNearestColor(Color, ColorMatchMode): �ȷ�Ʈ�� ���� ����� ������ ��ȯ�մϴ�.
- Color[] GetNearestColors(Color[] originalColors): �ȷ�Ʈ�� ���� ����� ������ ��ȯ�մϴ�.
- Color[] GetNearestColors(Texture, ColorMatchMode, ...): �ȷ�Ʈ�� ������� �ؽ�ó ���� �� ���� ����� ������ ��ȯ�մϴ�.
- Texture2D GetNearestTexture(Texture, ColorMatchMode, ...): ���� �ȷ�Ʈ�� ������� �־��� �ؽ�ó�� ��ȯ�մϴ�.


Support
-------
https://kronnect.com�� ���� �����̳� ���̵忡�� ������ ������ ��ũ��Ʈ ������ ã�ƺ�����.

Have any question or issue?
* Support-Web: https://kronnect.com/support
* Support-Discord: https://discord.gg/EH2GMaM
* Email: contact@kronnect.com
* Twitter: @Kronnect


Version history
---------------

Version 4.1.2
- Added support for Unity 2023.3

Version 4.1.1
- [Fix] Fixed "Export Texture" command producing a non-expected texture resolution

Version 4.1
- Added 10 sample palettes under Color Studio/Samples folder

Version 4
- Recolor optimization: a new option "Enable Optimization" can be found in the inspector when recolor mode is set to texture. This option creates an internal temporary LUT which matches all color transformations, improving the speed dramatically. Note: this LUT must be updated pressing "Update Optimization" in Recolor inspector when changing the color transformations.

Version 3.2.1
- [Fix] Fixed an issue when using ReColor to change only the main color and model has no texture or has incompatible compression mode

Version 3.2
- Added new Bake method to ReColor capable of exporting separate material/texture files

Version 3.1.1
- [Fix] Fixes for Unity 2021.3

Version 3.1
- Added "Bake" option to ReColor component. Let you store modifications into the gameobject permanently and remove recolor component

Version 3.0.1
- Recolor memory optimizations

Version 3.0
- Added zoom support

Version 2.9
- API: added CSPalette.ConfigurePalette(...) allows custom palette creation with default key colors (see https://kronnect.com/support/index.php/topic,5320.0.html)

Version 2.8
- Added "Pick Original Color from Scene View" button in Recolor inspector. Grabs the exact original color by clicking on the object in SceneView which is more accurate than using the color picker
- Fixes and optimizations

Version 2.7
- Allows more compact layout

Version 2.6
- Added automatic scrollbars to Pixel Painter window

Version 2.5
- Color wheel tab: options for entering primary color values directly
- Redesigned Palette RGB input section using a single line and a dropdown for color encoding option

Version 2.4
- Support for linear (non SRGB) textures

Version 2.3.2
- [Fix] Fixed wrong output size when saving a texture after resizing a canvas
- [Fix] Updated links to support site

Version 2.3.1
- [Fix] Fixed build issue with addressables

Version 2.3
- Added "Save As New" separate button to clear confusion when duplicating an open palette
- Fixes

Version 2.2
- Recolor scripts now update automatically when palette changes are saved from Color Studio window
- Prevents a runtime error when some material doesn't exist or has been disposed in a renderer with multiple materials
- Ensure palette preview is refreshed in inspector
- Removes a console warning when moving the color threshold slider in the inspector of Recolor script

Version 2.1
- Improved performance of Recolor operations

Version 2.0.3
- [Fix] Fixed quad-mode mirror drawing issue

Version 2.0
- New "Pixel Painter" window

Version 1.5
- Recolor: LUT performance optimizations

Version 1.4
- Recolor: added LUT option
- Recolor: added Color Correction options (vibrance, contrast, brightness, tinting)

Version 1.3
- Recolor now supports vertex colors transformations

Version 1.2
- Added Color Threshold option
- Realtime preview in Editor
- Added command button to add main texture colors to Color Operations section

Version 1.1
- Added Color Match mode option
- Can specify custom color operations

Version 1.0
- Initial version




