﻿using Microsoft.AspNetCore.Html;

namespace Plato.Internal.Assets.Abstractions
{
    
    public class Asset
    {

        public string Url { get; set; }

        public string CdnUrl { get; set; }

        public AssetType Type { get; set; }

        public AssetSection Section { get; set; }

        public int Priority { get; set; }

        public IHtmlContent InlineContent { get; set; }
    }
    
    public enum AssetSection
    {
        Meta,
        Header,
        Body,
        Footer
    }

    public enum AssetType
    {
        IncludeJavaScript,
        IncludeCss,
        Meta
    }

}
