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

        public string PotentialZombiesListMessage { get; set; } = "Potential zombies:<br><br>{0}";

        public string PotentialZombiesListElement { get; set; } = "<color=yellow>{0}</color> - <color=yellow>{1}</color>m away - <color=yellow>{2}</color>s";

        public string DisabledCommand { get; set; } = "This command is disabled on this server";

        public string WrongSideCommandInfo { get; set; } = "Only Foundation Personnel (MTF, Guards, Sciencists) or Chaos Insurgents can use this command";

        public string ExceededCuffingLimit { get; set; } = "You have reached your cuffing limit";

        public string NoScpNearby { get; set; } = "There is no SCP-049 nearby";

        public string UncuffedScpCommandInfo { get; set; } = "Uncuffed nearby SCP 049";

        public string AlreadyBeingDisarmed { get; set; } = "SCP-049 is already in disarming process";

        public string InProgressCommandInfo { get; set; } = "In progress";

        public string DisarmingMessage049 { get; set; } = "<color=red><size=150%>You are being disarmed</size></color><br>Stand still for <color=yellow>{0}</color>s";

        public string DisarmingFailedMessage049 { get; set; } = "<color=red><size=150%>Disarming canceled</size></color>";

        public string DisarmingFailedMessageCuffer { get; set; } = "<color=red><size=150%>Disarming canceled</size></color><br>SCP-049 <color=yellow>moved</color>";

        public string DisarmingSuccessfull { get; set; } = "Disarming <color=green>successfull</color>";

        public string DisarmedInformation049 { get; set; } = "<br><br><size=200%><color=red>You are disarmed!</color></size><br>Your cuffer is: <color=yellow>{0}</color><br>Follow orders!";

        public string ContainingMessage049 { get; set; } = "<color=red><size=150%>You are being recontained</size></color><br>Stand still for <color=yellow>{0}</color>s";

        public string ContainingMessageCuffer { get; set; } = "<color=red>SCP-049</color> is being <color=yellow>Recontained</color> (<color=yellow>{0}</color>s)";

        public string ContainingFailedMessage049 { get; set; } = "<color=red><size=150%>Recontainment canceled</size></color>";

        public string ContainingFailedMessageCuffer { get; set; } = "<color=red><size=150%>Recontainment canceled</size></color><br><color=red>SCP-049</color> left the <color=yellow>Containment Chamber</color>";
    }
}
