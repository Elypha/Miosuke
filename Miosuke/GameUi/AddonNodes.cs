using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Miosuke.GameUi;

public static unsafe class AddonNodes
{
    // Use the NodeList scan for dynamically spliced plugin-owned nodes. Built-in lookup can miss those nodes.
    public static AtkResNode* FindNodeInNodeList(AtkUnitBase* addon, uint nodeId)
    {
        if (addon == null || addon->UldManager.NodeList == null) return null;

        for (var i = 0; i < addon->UldManager.NodeListCount; i++)
        {
            var node = addon->UldManager.NodeList[i];
            if (node == null || node->NodeId != nodeId) continue;

            return node;
        }

        return null;
    }

    public static AtkTextNode* FindTextNodeInNodeList(AtkUnitBase* addon, uint nodeId)
    {
        var node = FindNodeInNodeList(addon, nodeId);
        return node == null || node->Type != NodeType.Text
            ? null
            : (AtkTextNode*)node;
    }

    public static AtkResNode* FindNodeByAddonLookup(AtkUnitBase* addon, uint nodeId) =>
        addon == null ? null : addon->GetNodeById(nodeId);

    public static AtkTextNode* FindTextNodeByAddonLookup(AtkUnitBase* addon, uint nodeId) =>
        addon == null ? null : addon->GetTextNodeById(nodeId);

    public static AtkResNode* SearchNodeByUldManager(AtkUnitBase* addon, uint nodeId) =>
        addon == null ? null : addon->UldManager.SearchNodeById(nodeId);

    public static AtkTextNode* CreateTextNodeFromSourceBefore(
        AtkUnitBase* addon,
        AtkTextNode* sourceNode,
        AtkResNode* insertBeforeNode,
        uint nodeId,
        byte lineSpacing = 18,
        byte fontSize = 12,
        bool updateDrawNodeList = true)
    {
        if (addon == null
            || sourceNode == null
            || insertBeforeNode == null)
        {
            return null;
        }

        if (FindNodeInNodeList(addon, nodeId) != null) return null;

        var textNode = IMemorySpace.GetUISpace()->Create<AtkTextNode>();
        if (textNode == null) return null;

        textNode->AtkResNode.Type = NodeType.Text;
        textNode->AtkResNode.NodeId = nodeId;
        textNode->AtkResNode.NodeFlags = NodeFlags.AnchorLeft | NodeFlags.AnchorTop;
        textNode->AtkResNode.DrawFlags = 0;
        textNode->AtkResNode.Color = sourceNode->AtkResNode.Color;
        textNode->TextColor = sourceNode->TextColor;
        textNode->EdgeColor = sourceNode->EdgeColor;
        textNode->LineSpacing = lineSpacing;
        textNode->AlignmentFontType = 0x00;
        textNode->FontSize = fontSize;
        textNode->TextFlags = sourceNode->TextFlags | TextFlags.MultiLine | TextFlags.AutoAdjustNodeSize;
        textNode->AtkResNode.ToggleVisibility(false);
        if (!TryLinkNodeBefore(addon, (AtkResNode*)textNode, insertBeforeNode, updateDrawNodeList))
        {
            textNode->AtkResNode.Destroy(true);
            return null;
        }

        return textNode;
    }

    public static AtkNineGridNode* CreateNineGridNodeFromSourceBefore(
        AtkUnitBase* addon,
        AtkNineGridNode* sourceNode,
        AtkResNode* insertBeforeNode,
        uint nodeId,
        bool updateDrawNodeList = true)
    {
        if (addon == null
            || sourceNode == null
            || insertBeforeNode == null)
        {
            return null;
        }

        if (FindNodeInNodeList(addon, nodeId) != null) return null;

        var nineGridNode = CreateNineGridNodeFromSource(sourceNode, nodeId);
        if (nineGridNode == null) return null;

        nineGridNode->AtkResNode.ToggleVisibility(false);
        if (!TryLinkNodeBefore(addon, (AtkResNode*)nineGridNode, insertBeforeNode, updateDrawNodeList))
        {
            nineGridNode->AtkResNode.Destroy(true);
            return null;
        }

        return nineGridNode;
    }

    public static void UpdateDrawNodeList(AtkUnitBase* addon)
    {
        if (addon == null) return;

        addon->UldManager.UpdateDrawNodeList();
    }

    public static void DestroyLinkedTextNode(AtkUnitBase* addon, AtkTextNode* textNode)
    {
        DestroyLinkedNode(addon, (AtkResNode*)textNode);
    }

    public static void DestroyLinkedNode(AtkUnitBase* addon, AtkResNode* node)
    {
        if (addon == null || node == null) return;

        if (node->PrevSiblingNode != null) node->PrevSiblingNode->NextSiblingNode = node->NextSiblingNode;
        if (node->NextSiblingNode != null) node->NextSiblingNode->PrevSiblingNode = node->PrevSiblingNode;

        addon->UldManager.UpdateDrawNodeList();
        node->Destroy(true);
    }

    private static AtkNineGridNode* CreateNineGridNodeFromSource(AtkNineGridNode* sourceNode, uint nodeId)
    {
        var nineGridNode = IMemorySpace.GetUISpace()->Create<AtkNineGridNode>();
        if (nineGridNode == null) return null;

        CopyResNodeVisualState(&nineGridNode->AtkResNode, &sourceNode->AtkResNode, nodeId, NodeType.NineGrid);
        nineGridNode->PartsList = sourceNode->PartsList;
        nineGridNode->PartId = sourceNode->PartId;
        nineGridNode->TopOffset = sourceNode->TopOffset;
        nineGridNode->BottomOffset = sourceNode->BottomOffset;
        nineGridNode->LeftOffset = sourceNode->LeftOffset;
        nineGridNode->RightOffset = sourceNode->RightOffset;
        nineGridNode->BlendMode = sourceNode->BlendMode;
        nineGridNode->PartsTypeRenderType = sourceNode->PartsTypeRenderType;

        return nineGridNode;
    }

    private static void CopyResNodeVisualState(
        AtkResNode* targetNode,
        AtkResNode* sourceNode,
        uint nodeId,
        NodeType nodeType)
    {
        targetNode->Type = nodeType;
        targetNode->NodeId = nodeId;
        targetNode->NodeFlags = sourceNode->NodeFlags;
        targetNode->DrawFlags = sourceNode->DrawFlags;
        targetNode->Color = sourceNode->Color;
        targetNode->Depth = sourceNode->Depth;
        targetNode->Depth_2 = sourceNode->Depth_2;
        targetNode->AddRed = sourceNode->AddRed;
        targetNode->AddGreen = sourceNode->AddGreen;
        targetNode->AddBlue = sourceNode->AddBlue;
        targetNode->AddRed_2 = sourceNode->AddRed_2;
        targetNode->AddGreen_2 = sourceNode->AddGreen_2;
        targetNode->AddBlue_2 = sourceNode->AddBlue_2;
        targetNode->MultiplyRed = sourceNode->MultiplyRed;
        targetNode->MultiplyGreen = sourceNode->MultiplyGreen;
        targetNode->MultiplyBlue = sourceNode->MultiplyBlue;
        targetNode->MultiplyRed_2 = sourceNode->MultiplyRed_2;
        targetNode->MultiplyGreen_2 = sourceNode->MultiplyGreen_2;
        targetNode->MultiplyBlue_2 = sourceNode->MultiplyBlue_2;
        targetNode->Width = sourceNode->Width;
        targetNode->Height = sourceNode->Height;
        targetNode->OriginX = sourceNode->OriginX;
        targetNode->OriginY = sourceNode->OriginY;
        targetNode->Priority = sourceNode->Priority;
        targetNode->SetScale(sourceNode->ScaleX, sourceNode->ScaleY);
        targetNode->SetRotation(sourceNode->Rotation);
        targetNode->SetPositionFloat(sourceNode->X, sourceNode->Y);
        targetNode->ToggleVisibility(sourceNode->IsVisible());
    }

    public static bool TryLinkNodeBefore(
        AtkUnitBase* addon,
        AtkResNode* node,
        AtkResNode* insertBeforeNode,
        bool updateDrawNodeList = true)
    {
        if (addon == null
            || node == null
            || insertBeforeNode == null
            || insertBeforeNode->ParentNode == null)
        {
            return false;
        }

        var previousNode = insertBeforeNode->PrevSiblingNode;

        node->ParentNode = insertBeforeNode->ParentNode;
        node->PrevSiblingNode = previousNode;
        node->NextSiblingNode = insertBeforeNode;
        insertBeforeNode->PrevSiblingNode = node;

        // Do not update ParentNode->ChildNode here; Atk draw-list refresh accepts this sibling splice,
        // while changing the parent child pointer breaks some addon roots.
        if (previousNode != null) previousNode->NextSiblingNode = node;

        if (updateDrawNodeList) UpdateDrawNodeList(addon);

        return true;
    }
}
