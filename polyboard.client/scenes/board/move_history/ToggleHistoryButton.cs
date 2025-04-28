using Godot;
using System;

public partial class ToggleHistoryButton : Button
{
    [Export] public NodePath moveHistoryPath;
    
    private Control moveHistoryPanel;
    private bool isHistoryVisible = true;
    
    public override void _Ready()
    {
        moveHistoryPanel = GetNodeOrNull<Control>(moveHistoryPath);
        
        if (moveHistoryPanel == null)
        {
            GD.PrintErr("Błąd: Nie znaleziono panelu historii ruchów.");
            return;
        }
        
        // Domyślnie pokaż historię
        moveHistoryPanel.Visible = isHistoryVisible;
        
        // Zastosuj styl do przycisku
        ApplyButtonStyle();
        
        // Połącz sygnał kliknięcia
        Connect("pressed", new Callable(this, nameof(OnToggleButtonPressed)));
        
        // Ustaw początkowy tekst
        UpdateButtonText();
    }
    
    private void ApplyButtonStyle()
    {
        // Utwórz styl przycisku
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        buttonStyle.BorderWidthBottom = 3;
        buttonStyle.BorderWidthLeft = 3;
        buttonStyle.BorderWidthRight = 3;
        buttonStyle.BorderWidthTop = 3;
        buttonStyle.BorderColor = new Color("#62ff45");
        buttonStyle.CornerRadiusBottomLeft = 8;
        buttonStyle.CornerRadiusBottomRight = 8;
        buttonStyle.CornerRadiusTopLeft = 8;
        buttonStyle.CornerRadiusTopRight = 8;
        buttonStyle.ContentMarginLeft = 10;
        buttonStyle.ContentMarginRight = 10;
        buttonStyle.ContentMarginTop = 8;
        buttonStyle.ContentMarginBottom = 8;
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.95f);
        hoverStyle.BorderWidthBottom = 3;
        hoverStyle.BorderWidthLeft = 3;
        hoverStyle.BorderWidthRight = 3;
        hoverStyle.BorderWidthTop = 3;
        hoverStyle.BorderColor = new Color("#62ff45");
        hoverStyle.CornerRadiusBottomLeft = 8;
        hoverStyle.CornerRadiusBottomRight = 8;
        hoverStyle.CornerRadiusTopLeft = 8;
        hoverStyle.CornerRadiusTopRight = 8;
        hoverStyle.ContentMarginLeft = 10;
        hoverStyle.ContentMarginRight = 10;
        hoverStyle.ContentMarginTop = 8;
        hoverStyle.ContentMarginBottom = 8;
        
        AddThemeStyleboxOverride("normal", buttonStyle);
        AddThemeStyleboxOverride("hover", hoverStyle);
        AddThemeStyleboxOverride("pressed", buttonStyle);
        AddThemeColorOverride("font_color", new Color(1, 1, 1));
        AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
        AddThemeFontSizeOverride("font_size", 16);
    }
    
    private void OnToggleButtonPressed()
    {
        if (moveHistoryPanel == null) return;
        
        isHistoryVisible = !isHistoryVisible;
        moveHistoryPanel.Visible = isHistoryVisible;
        UpdateButtonText();
    }
    
    private void UpdateButtonText()
    {
        Text = isHistoryVisible ? "Ukryj historię" : "Pokaż historię";
    }
}