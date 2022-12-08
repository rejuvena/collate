namespace Rejuvena.Collate.Util;

internal static class JsEngineFactory
{
    public static Jint.Engine CreateModuleEngine() {
        return new Jint.Engine().Execute("module = []");
    }
}
