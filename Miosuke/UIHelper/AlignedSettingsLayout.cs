namespace Miosuke.UiHelper;

/// <summary>
/// Keeps label columns aligned across multiple two-column settings tables.
/// Labels are measured while they are drawn, so callers do not need to maintain
/// a separate list of labels just for layout. Width changes settle on the next
/// frame, which fits ImGui's immediate-mode model.
/// </summary>
public sealed class AlignedSettingsLayout
{
    private const float DefaultFallbackLabelColumnWidth = 150f;
    private const float DefaultLabelColumnPadding = 24f;

    private readonly Dictionary<string, float> _labelColumnWidths = [];

    public Scope Begin(
        string id,
        float fallbackLabelColumnWidth = DefaultFallbackLabelColumnWidth,
        float labelColumnPadding = DefaultLabelColumnPadding)
    {
        return new Scope(this, id, fallbackLabelColumnWidth, labelColumnPadding);
    }

    public sealed class Scope(AlignedSettingsLayout owner, string id, float fallbackLabelColumnWidth, float labelColumnPadding) : IDisposable
    {
        private float _measuredLabelColumnWidth;

        public float LabelColumnWidth
        {
            get
            {
                var cachedWidth = owner._labelColumnWidths.GetValueOrDefault(id, fallbackLabelColumnWidth);
                return MathF.Ceiling(Math.Max(cachedWidth, _measuredLabelColumnWidth));
            }
        }

        public ImRaii.TableDisposable BeginTable(
            string tableId,
            ImGuiTableFlags flags = ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.PadOuterX)
        {
            var table = ImRaii.Table(tableId, 2, flags, new Vector2(-1, 0));
            if (table)
            {
                ImGui.TableSetupColumn("Setting", ImGuiTableColumnFlags.WidthFixed, LabelColumnWidth);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
            }

            return table;
        }

        public void BeginRow(string label)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            DrawLabel(label);
            ImGui.TableNextColumn();
        }

        public void BeginRow(string label, Vector4 labelColour)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            DrawLabel(label, labelColour);
            ImGui.TableNextColumn();
        }

        public void DrawLabel(string label)
        {
            MeasureLabel(label);
            ImGui.TextUnformatted(label);
        }

        public void DrawLabel(string label, Vector4 colour)
        {
            MeasureLabel(label);
            ImGui.TextColored(colour, label);
        }

        public void MeasureLabel(string label)
        {
            var labelWidth = ImGui.CalcTextSize(label).X + labelColumnPadding;
            _measuredLabelColumnWidth = Math.Max(_measuredLabelColumnWidth, labelWidth);
        }

        public void Dispose()
        {
            if (_measuredLabelColumnWidth <= 0) return;

            owner._labelColumnWidths[id] = MathF.Ceiling(Math.Max(fallbackLabelColumnWidth, _measuredLabelColumnWidth));
        }
    }
}
