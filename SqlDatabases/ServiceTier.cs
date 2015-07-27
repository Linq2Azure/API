namespace Linq2Azure.SqlDatabases
{
    public enum ServiceTier
    {
        [ServiceTierMetadata(Edition.Web, "")]
        Web,
        [ServiceTierMetadata(Edition.Business, "")]
        Business,
        [ServiceTierMetadata(Edition.Basic, "f1173c43-91bd-4aaa-973c-54e79e15235b")]
        Basic,
        [ServiceTierMetadata(Edition.Standard, "f1173c43-91bd-4aaa-973c-54e79e15235b")]
        StandardS0,
        [ServiceTierMetadata(Edition.Standard, "1b1ebd4d-d903-4baa-97f9-4ea675f5e928")]
        StandardS1,
        [ServiceTierMetadata(Edition.Standard, "455330e1-00cd-488b-b5fa-177c226f28b7")]
        StandardS2,
        [ServiceTierMetadata(Edition.Standard, "789681b8-ca10-4eb0-bdf2-e0b050601b40")]
        StandardS3,
        [ServiceTierMetadata(Edition.Premium, "7203483a-c4fb-4304-9e9f-17c71c904f5d")]
        PremiumS1,
        [ServiceTierMetadata(Edition.Premium, "a7d1b92d-c987-4375-b54d-2b1d0e0f5bb0")]
        PremiumS2,
        [ServiceTierMetadata(Edition.Premium, "a7c4c615-cfb1-464b-b252-925be0a19446")]
        PremiumS3
    }
}