namespace MockHttp.Services;

public interface IImageService
{
    byte[] GetPng();
    byte[] GetJpeg();
}
