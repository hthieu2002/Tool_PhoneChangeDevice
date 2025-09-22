using POCO.Models;

namespace POCO.ResponseModels

{
    public class GetDeviceWadogeNewResponse
    {
        public DeviceModel GetDeviceWadogeNew { get; set; }

    }
    public sealed class ListBrandsResponse
    {
        // do mình alias Brands: listBrands
        public List<string> Brands { get; set; }
    }
}
