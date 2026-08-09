public override Metadata? Metadata =>
    new(1, "Maggie Ifrit Dash accessibility build");

private bool active;
private long startedAt;
private int stage;

public override void OnSetup()
{
    Controller.RegisterElementFromCode(
        "IfritDash_Current",
        """{"Name":"CURRENT","Enabled":false,"radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"tether":true}"""
    );

    Controller.RegisterElementFromCode(
        "IfritDash_Next",
        """{"Name":"NEXT","Enabled":false,"radius":2.2,"Donut":0.35,"color":4294967040,"thicc":8.0,"tether":true}"""
    );

    OnReset();
}

public override void OnStartingCast(uint source, uint castId)
{
    // Ultimate Annihilation
    if (castId != 0x2D4C)
        return;

    active = true;
    startedAt = Environment.TickCount64;
    stage = 0;
    DisableMarkers();
}

public override void OnUpdate()
{
    if (!active)
        return;

    var elapsed = Environment.TickCount64 - startedAt;

    // Prepare for Crimson Cyclone.
    // Green = current position.
    // Cyan = beginning of the Ifrit dash.
    if (stage == 0 && elapsed >= 23000)
    {
        stage = 1;

        ShowRoute(
            new Vector3(87.332f, 0.0f, 87.270f),
            new Vector3(100.138f, 0.0f, 81.841f)
        );

        return;
    }

    // Crimson Cyclone.
    // Green = beginning of dash.
    // Cyan = end of dash.
    if (stage == 1 && elapsed >= 27500)
    {
        stage = 2;

        ShowRoute(
            new Vector3(100.138f, 0.0f, 81.841f),
            new Vector3(100.070f, 0.0f, 90.900f)
        );

        return;
    }

    // Remove markers after the dash.
    if (elapsed >= 33000)
        Controller.Reset();
}

public override void OnReset()
{
    active = false;
    startedAt = 0;
    stage = 0;
    DisableMarkers();
}

private void ShowRoute(Vector3 currentPosition, Vector3 nextPosition)
{
    if (!Controller.TryGetElementByName(
            "IfritDash_Current",
            out var current) ||
        !Controller.TryGetElementByName(
            "IfritDash_Next",
            out var next))
        return;

    current.SetRefPosition(currentPosition);
    next.SetRefPosition(nextPosition);

    current.Enabled = true;
    next.Enabled = true;
}

private void DisableMarkers()
{
    if (Controller.TryGetElementByName(
            "IfritDash_Current",
            out var current))
        current.Enabled = false;

    if (Controller.TryGetElementByName(
            "IfritDash_Next",
            out var next))
        next.Enabled = false;
}
