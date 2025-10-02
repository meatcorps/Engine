namespace Meatcorps.Game.Web.TruthOrDare.Utilities;

public class QrCodeHelper
{
    public static string SvgQrCode(string payload, int size = 10)
    {
        return new QRCoder.SvgQRCode(
                new QRCoder.QRCodeGenerator()
                    .CreateQrCode(payload, QRCoder.QRCodeGenerator.ECCLevel.Q))
            .GetGraphic(size, "#000000", "#FFFFFF", true);
    }
}