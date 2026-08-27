// O projeto compila em net472 (perfil Mono do jogo), mas usa LangVersion=latest.
// Recursos modernos de C# como 'record' e propriedades 'init' exigem o tipo
// IsExternalInit, que so existe a partir do .NET 5. Este shim o declara para
// que esses recursos funcionem sem sair do net472.
//
// Nao remova: sem isso, 'init' e 'record' viram erro de compilacao.

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
