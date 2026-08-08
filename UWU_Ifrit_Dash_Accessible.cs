ChatGPT Plus





or accessible

Worked for 24s
Yes — “Accessible” is the clue. I found the exact prior files.

We had a GitHub repo called FFXIV-Splatoon-Scripts, and inside it we created files such as UWU_Predation_Accessible_Dynamic_v1.cs and UWU_Annihilation_D3_Accessible.cs. Your screenshots show those files actually sitting in the repo. 

 


So for this one, don’t upload the downloaded file yet. We’ll repeat the browser-created-file method:

Open your FFXIV-Splatoon-Scripts GitHub repo.

Click Add file.

Choose Create new file.

Name it exactly:

UWU_Ifrit_Dash_Accessible.cs

Leave the big code box open.

Once you are there, tell me “I’m at the code box” and I’ll give you the exact code to paste. That matches the workflow where we created UWU_Annihilation_D3_Accessible.cs directly in GitHub. 



im at code box after that I will get you raw code what do I put there

Perfect. In the big GitHub code box, paste the entire contents of the .cs file I made — not the raw URL yet.

Easiest way:

Open the file you downloaded: UWU_Ifrit_Dash_Accessible.cs

Press Ctrl+A

Press Ctrl+C

Go back to the big GitHub code box.

Press Ctrl+V.

Scroll down.

Click Commit changes.

Choose Commit directly to the main branch.

Click the green Commit changes.

Your filename at the top should remain:

UWU_Ifrit_Dash_Accessible.cs

Do not change anything in the code.

After it commits, open UWU_Ifrit_Dash_Accessible.cs in GitHub and click Raw.

Then send me that Raw URL. I’ll check it before we put it into Splatoon.


link the correct thing I can put there

Yes — use the actual file I created for you. Open/download this:

UWU_Ifrit_Dash_Accessible.cs

Then:

Open it.

Ctrl+A → Ctrl+C

Go to the big GitHub code box.

Ctrl+V

Click Commit changes.

The GitHub filename should be exactly:

UWU_Ifrit_Dash_Accessible.cs

Once committed, click Raw and send me that link.


UWU_Ifrit_Dash_Accessible.cs
Code


Library
/
UWU_Ifrit_Dash_Accessible.cs


using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UWU_Ifrit_Dash_Accessible : SplatoonScript
{
    private const uint CrimsonCycloneCastId = 0x2B5F;
    private static readonly Vector3 Center = new(100f, 0f, 100f);

    private bool active;
    private long startedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [Raids.the_Weapons_Refrain_Ultimate];

    public override Metadata? Metadata =>
        new(1, "Maggie — UWU Ifrit Dash accessibility");

    public override void OnSetup()
    {
        // Bright green = Ifrit's dash START.
        Controller.RegisterElementFromCode(
            "IfritDash_Current",
            "{\"Name\":\"CURRENT\",\"Enabled\":false,\"radius\":2.5,\"Donut\":0.35,\"color\":4278255360,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true}"
        );

        // Bright cyan = opposite side / dash END.
        Controller.RegisterElementFromCode(
            "IfritDash_Next",
            "{\"Name\":\"NEXT\",\"Enabled\":false,\"radius\":2.2,\"Donut\":0.35,\"color\":4294967040,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true}"
        );

        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId != CrimsonCycloneCastId)
            return;

        var ifrit = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x => x.EntityId == source);

        if (ifrit == null)
            return;

        var start = ifrit.Position;
        var end = OppositePoint(start);

        ShowRoute(start, end);

        active = true;
        startedAt = Environment.TickCount64;
    }

    public override void OnUpdate()
    {
        if (!active)
            return;

        // Keep the two markers visible briefly through the dash.
        if (Environment.TickCount64 - startedAt > 4500)
            OnReset();
    }

    public override void OnReset()
    {
        active = false;
        startedAt = 0;
        DisableMarkers();
    }

    private static Vector3 OppositePoint(Vector3 start)
    {
        // Mirror Ifrit's starting position through arena center.
        return new Vector3(
            Center.X * 2f - start.X,
            start.Y,
            Center.Z * 2f - start.Z
        );
    }

    private void ShowRoute(Vector3 currentPosition, Vector3 nextPosition)
    {
        if (!Controller.TryGetElementByName("IfritDash_Current", out var current) ||
            !Controller.TryGetElementByName("IfritDash_Next", out var next))
            return;

        current.SetRefPosition(currentPosition);
        next.SetRefPosition(nextPosition);

        current.Enabled = true;
        next.Enabled = true;
    }

    private void DisableMarkers()
    {
        if (Controller.TryGetElementByName("IfritDash_Current", out var current))
            current.Enabled = false;

        if (Controller.TryGetElementByName("IfritDash_Next", out var next))
            next.Enabled = false;
    }
}
