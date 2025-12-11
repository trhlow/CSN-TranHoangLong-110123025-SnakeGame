using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Game/Color Palette", order = 1)]
public class ColorPalette : ScriptableObject
{
    public static ColorPalette Instance { get; private set; }

    [Header("Player 1 Colors")]
    public Color player1Primary = new Color(0f, 1f, 0f);
    public Color player1Secondary = new Color(0.4f, 1f, 0.4f);
    public Color player1Glow = new Color(0f, 1f, 0f, 0.5f);

    [Header("Player 2 Colors")]
    public Color player2Primary = new Color(1f, 0f, 1f);
    public Color player2Secondary = new Color(1f, 0.4f, 1f);
    public Color player2Glow = new Color(1f, 0f, 1f, 0.5f);

    [Header("AI Colors")]
    public Color aiPrimary = new Color(0f, 1f, 1f);
    public Color aiSecondary = new Color(0.4f, 1f, 1f);
    public Color aiGlow = new Color(0f, 1f, 1f, 0.5f);

    [Header("Food Colors")]
    public Color foodCommon = new Color(1f, 0.13f, 0.27f);
    public Color foodRare = new Color(1f, 0.84f, 0f);
    public Color foodEpic = new Color(0f, 1f, 1f);

    [Header("Grid Colors")]
    public Color gridMain = new Color(0f, 1f, 1f, 0.3f);
    public Color gridSub = new Color(0f, 1f, 1f, 0.1f);
    public Color gridBorder = new Color(1f, 1f, 1f, 0.8f);

    [Header("Background Colors")]
    public Color backgroundTop = new Color(0.06f, 0.05f, 0.16f);
    public Color backgroundMid = new Color(0.19f, 0.17f, 0.39f);
    public Color backgroundBot = new Color(0.14f, 0.14f, 0.24f);

    [Header("UI Colors")]
    public Color uiAccent = new Color(0f, 1f, 1f);
    public Color uiPrimary = new Color(0.2f, 0.56f, 0.86f);
    public Color uiDanger = new Color(0.91f, 0.3f, 0.24f);
    public Color uiSuccess = new Color(0.18f, 0.8f, 0.44f);
    public Color uiText = Color.white;
    public Color uiTextDark = new Color(0.2f, 0.2f, 0.2f);

    private void OnEnable()
    {
        Instance = this;
    }

    public Color GetPlayerColor(int playerID)
    {
        return playerID switch
        {
            1 => player1Primary,
            2 => player2Primary,
            3 => aiPrimary,
            _ => Color.white
        };
    }

    public Color GetPlayerGlow(int playerID)
    {
        return playerID switch
        {
            1 => player1Glow,
            2 => player2Glow,
            3 => aiGlow,
            _ => Color.white
        };
    }
}