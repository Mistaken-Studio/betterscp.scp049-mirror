// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Interfaces;

namespace Mistaken.BetterSCP.SCP049
{
    internal class Translation : ITranslation
    {
        public string StartMessage { get; set; } = "<color=red><b><size=500%>UWAGA</size></b></color><br><br><br><br><br><br><size=90%>Rozgrywka jako <color=red>SCP 049</color> na tym serwerze jest zmodyfikowana, <color=red>SCP 049</color> posiada domyślnie dodatkowe <color=yellow>200</color> ahp, każdy <color=red>SCP 049-2</color> w zasięgu <color=yellow>10</color> metrów dodaje +<color=yellow>100</color> do max ahp, ahp regeneruje się z prędkością <color=yellow>20</color> na sekundę pod warunkiem że jest <color=yellow>bezpieczny</color>(w ciągu ostatnich <color=yellow>10</color> sekund nie otrzymał obrażeń)</size>";

        public string ContainingMessage049 { get; set; } = "<color=red><size=150%>You are being recontained</size></color><br>Stand still for <color=yellow>{0}</color>s";

        public string ContainingMessageCuffer { get; set; } = "<color=red>SCP-049</color> is being <color=yellow>Recontained</color> (<color=yellow>{0}</color>s)";

        public string ContainingFailedMessage049 { get; set; } = "<color=red><size=150%>Recontainment canceled</size></color>";

        public string ContainingFailedMessageCuffer { get; set; } = "<color=red><size=150%>Recontainment canceled</size></color><br><color=red>SCP-049</color> left the <color=yellow>Containment Chamber</color>";
    }
}
