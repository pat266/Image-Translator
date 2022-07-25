# Image Translation (and Text Detection)

This project is a proof-of-concept project for the on-screen translation for my game launcher: https://github.com/pat266/game-launcher

## Libraries
* `EmguCV` for Text Detection
    * Draw Bounding Rectangle around the text
    * Change values (i.e. `ar`, `brect.Width`, `brect.Height`, etc.) located in `GetBoudingRectangles()` in `Form1.cs` to suit your image
* `IronOCR` for Text Extraction
    * Not using Tesseract since IronOCR gives higher accuracy
* `Google Translate` for Text Translation

## Installed Libraries
* Emgu.CV (4.4.0.4061)
* Emgu.CV.Bitmap (4.4.0.4061)
* Emgu.CV.runtime.windows (4.4.0.4061)
* IronOcr (2022.3.0)
* IronOcr.Languages.Chinese (2020.11.2)
* System.Drawing.Common (4.7.0)
* GTranslate (2.1.0)
    * Installing this from Nuget also installs a lot more lib

## Images

### Text Recognition
#### Original Image
![](./testImg/sample.PNG)

#### After
![](./testImg/result/sample_result.PNG)