using OSGeo.GDAL;
using System;

namespace Adadev.GdalModule {

    public class ReadWrite {

        public ReadWrite() {
            GdalConfiguration.ConfigureGdal();
        }

        public void ReadAndWriteImage(string imagePath, string outImagePath) {

            using(Dataset image = Gdal.Open(imagePath, Access.GA_ReadOnly)) {

                Band redBand = GetBand(image, ColorInterp.GCI_RedBand);
                Band greenBand = GetBand(image, ColorInterp.GCI_GreenBand);
                Band blueBand = GetBand(image, ColorInterp.GCI_BlueBand);
                Band alphaBand = GetBand(image, ColorInterp.GCI_AlphaBand);

                if(redBand == null || greenBand == null || blueBand == null || alphaBand == null) {
                    throw new NullReferenceException("One or more bands are not available.");
                }

                int width = redBand.XSize;
                int height = redBand.YSize;

                using(Dataset outImage = Gdal.GetDriverByName("GTiff").Create(outImagePath, width, height, 4, redBand.DataType, null)) {
                    // copia a projeção e informações geográficas da imagem
                    double[] geoTransformerData = new double[6];
                    image.GetGeoTransform(geoTransformerData);
                    outImage.SetGeoTransform(geoTransformerData);
                    outImage.SetProjection(image.GetProjection());

                    Band outRedBand = outImage.GetRasterBand(1);
                    Band outGreenBand = outImage.GetRasterBand(2);
                    Band outBlueBand = outImage.GetRasterBand(3);
                    Band outAlphaBand = outImage.GetRasterBand(4);

                    for(int h = 0; h < height; h++) {
                        int[] red = new int[width];
                        int[] green = new int[width];
                        int[] blue = new int[width];
                        int[] alpha = new int[width];
                        // copia cada linha da matriz da imagem para os vetores definidos acima
                        redBand.ReadRaster(0, h, width, 1, red, width, 1, 0, 0);
                        greenBand.ReadRaster(0, h, width, 1, green, width, 1, 0, 0);
                        blueBand.ReadRaster(0, h, width, 1, blue, width, 1, 0, 0);
                        alphaBand.ReadRaster(0, h, width, 1, alpha, width, 1, 0, 0);

                        for(int w = 0; w < width; w++) {
                            red[w] = red[w] + 1; // algum processo com cada pixel
                            green[w] = green[w] + 1;
                            blue[w] = blue[w] + 1;
                            alpha[w] = alpha[w] + 1;
                        }
                        // escrever imagem
                        outRedBand.WriteRaster(0, h, width, 1, red, width, 1, 0, 0);
                        outGreenBand.WriteRaster(0, h, width, 1, green, width, 1, 0, 0);
                        outBlueBand.WriteRaster(0, h, width, 1, blue, width, 1, 0, 0);
                        outAlphaBand.WriteRaster(0, h, width, 1, alpha, width, 1, 0, 0);
                    }
                    outImage.FlushCache();
                }
            }
        }

      /**
       * Retorna a banda para determinada cor (red, green, blue ou alfa)
       * O dataset deve ter 4 bandas
       * */
        public static Band GetBand(Dataset ImageDataSet, ColorInterp colorInterp) {
            if(colorInterp.Equals(ImageDataSet.GetRasterBand(1).GetRasterColorInterpretation())) {
                return ImageDataSet.GetRasterBand(1);
            } else if(colorInterp.Equals(ImageDataSet.GetRasterBand(2).GetRasterColorInterpretation())) {
                return ImageDataSet.GetRasterBand(2);
            } else if(colorInterp.Equals(ImageDataSet.GetRasterBand(3).GetRasterColorInterpretation())) {
                return ImageDataSet.GetRasterBand(3);
            } else {
                return ImageDataSet.GetRasterBand(4);
            }
        }
    }
}
