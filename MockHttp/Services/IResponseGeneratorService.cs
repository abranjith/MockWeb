namespace MockHttp.Services;

public interface IResponseGeneratorService
{
    object GetSimpleJson();
    object GetComplexJson();
    string GetHtmlString();
    XmlSample GetXmlObject();
    string GetPlainText();
    object GetUserProfile(int userId);
    object GetProductCatalog(int page, int pageSize);
    object GetOrderDetails(Guid orderId);
}
