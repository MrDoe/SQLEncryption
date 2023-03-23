using Microsoft.SqlServer.Server;
using Yahoo.Yui.Compressor;

namespace JSMinifier
{
    public class JSMinifier
    {
        [SqlFunction]
        public static string Minify(string jsCode)
        {
            if (jsCode == null)
                return "";

            var jsCompressor = new JavaScriptCompressor();
            return jsCompressor.Compress(jsCode);
        }
    }
}
