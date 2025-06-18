namespace POCO.ResponseModels

{
    public class ReleaseProxyModel
    {
        public MutationResponseModel ReleaseProxy { get; set; }
    }

    public class MutationResponseModel
    {
        public bool Result { get; set; }
        public string Message { get; set; }
    }
}
