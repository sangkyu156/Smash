****************
* COLOR STUDIO *
* README FILE  *
****************


이 자산을 사용하는 방법
---------------------
1) Unity Editor 내에서 최상위 메뉴 Window -> Color Studio -> Palette Manager 또는 Pixel Painter를 선택합니다.

2) 이 비디오에 표시된 대로 인터페이스 창을 끌어서 다른 창에 도킹하는 것이 좋습니다.
https://youtu.be/Qe2yHCqWhoU

3) ?를 클릭하세요. 빠른 지침을 위한 Color Studio 창의 탭

4) Pixel Painter 창을 사용하여 픽셀 아트 만들기

5) 런타임에 색상을 변경하려면 게임 개체나 스프라이트에 "Recolor" 스크립트를 추가하세요.



스크립팅 지원
-----------------

Color Studio는 UI 인터페이스를 갖춘 편집기 확장으로 설계되었지만 일부 기능은 스크립팅을 사용하여 활용할 수 있습니다.
특히 CSPalette 클래스는 다음과 같이 인스턴스화할 수 있습니다.

// 빨간색을 기본으로 하는 분할 보색 디자인을 기본으로 사용하는 팔레트를 만듭니다.
CSPalette palette = ScriptableObject.CreateInstance<CSPalette>();
palette.ConfigurePalette(ColorScheme.SplitComplementary, Color.red, splitAmount: 0.6f);

// 유채색 만들기
palette.BuildHueColors();

// 팔레트 색조 색상 출력
for (int k=0;k<palette.colorsCount;k++) {
    Debug.Log(palette.colors[k]);
}

// 팔레트 색상 구성(색상 포함)
palette.BuildPaletteColors();
for (int k=0;k<paletteColors.Length;k++) {
    Debug.Log(palette.paletteColors[k]);
}

기타 유용한 방법(메소드):

- Texture2D ExportLUT(): 현재 색상 팔레트를 기반으로 LUT를 구축합니다.
- Texture2D ExportTexture(): 색상 팔레트를 포함하는 텍스처를 만듭니다.
- Color ApplyLUT(Color, LUT): 주어진 LUT를 기반으로 색상을 변환합니다.
- Color GetNearestColor(Color, ColorMatchMode): 팔레트의 가장 가까운 색상을 반환합니다.
- Color[] GetNearestColors(Color[] originalColors): 팔레트의 가장 가까운 색상을 반환합니다.
- Color[] GetNearestColors(Texture, ColorMatchMode, ...): 팔레트를 기반으로 텍스처 색상 중 가장 가까운 색상을 반환합니다.
- Texture2D GetNearestTexture(Texture, ColorMatchMode, ...): 현재 팔레트를 기반으로 주어진 텍스처를 변환합니다.


Support
-------
https://kronnect.com의 지원 포럼이나 가이드에서 유용한 주제와 스크립트 샘플을 찾아보세요.

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




