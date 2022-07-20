# Image Translation (and Text Detection)

This project is a proof-of-concept project for the on-screen translation for my game launcher: https://github.com/pat266/game-launcher

## Process
* `EmguCV` for Text Detection
    * Draw Bounding Rectangle around the text
* `IronOCR` for Text Extraction
    * Not using Tesseract since IronOCR gives higher accuracy
* `Google Translate` for Text Translation

## Images

### Text Recognition
#### Original
![](./testImg/sample.PNG)

#### After
![](./testImg/result/sample_result.PNG)