using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RitualDrawingComparison : MonoBehaviour
{
    public static RitualDrawingComparison Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("Multiple instances of RitualDrawingComparison found! There should only be one.");
            Destroy(gameObject);
        }
        Debug.Log("RitualDrawingComparison Awake complete");
        gameObject.SetActive(false);
    }


    [Header("References")]
    public Texture2D targetShapeTexture;
    public GameObject outline;
    [SerializeField] private Texture2D switchRoomShape;
    [SerializeField] private Texture2D rotateRoomShape;
    [SerializeField] private Texture2D flashRoomShape;

    [Header("Comparison Settings")]
    [Range(0f,1f)]
    public float requiredAccuracy = 0.8f;

    [Range(0f, 1f)]
    public float penaltyWeight = 0.1f; //How much to penalize extra pixels

    [Header("Ritual Paper Mode")]
    private RitualPaperItem currentPaper = null;
    private GameObject currentPaperObject = null;
    private Texture2D targetTexture;
    private Texture2D resizedTarget;

    [Header("Debug")]
    public bool showDebugInfo = true;
    // Data class to hold comparison results
    [System.Serializable]
    public class ComparisonResult
    {
        public float accuracy;          // Final score (0-1)
        public float coverage;          // How much of target was drawn (0-1)
        public float precision;
        public float extraPenalty;      // Penalty for drawing outside lines (0+)
        public int matchingPixels;      // Pixels correctly drawn
        public int totalTargetPixels;   // Total pixels in target shape
        public int extraPixels;         // Pixels drawn outside target
        public int drawnPixels;
        public bool isSuccessful;       // Did they pass the threshold?
    }

    void PrepareTargetTexture(SpellType spell)
    {
        targetShapeTexture = spell switch
        {
            SpellType.ROTATE_ROOM => rotateRoomShape,
            SpellType.SWITCH_ROOM => switchRoomShape,
            SpellType.FLASH => flashRoomShape,
            _ => null
        };

        // Get the size of the draw canvas
        RectTransform drawRect = DrawScript.Instance.DrawImage.rectTransform;
        int targetWidth = Mathf.RoundToInt(drawRect.rect.width);
        int targetHeight = Mathf.RoundToInt(drawRect.rect.height);

        // Resize target texture to match draw canvas if needed
        if (targetShapeTexture.width != targetWidth || targetShapeTexture.height != targetHeight)
        {

            resizedTarget = ResizeTexture(targetShapeTexture, targetWidth, targetHeight);

            outline.GetComponent<RawImage>().texture = resizedTarget;
            targetTexture = resizedTarget;
        }
        else
        {
            outline.GetComponent<RawImage>().texture = targetShapeTexture;
            targetTexture = targetShapeTexture;
        }
        targetTexture.Apply();
    }

    ///// <summary>
    ///     Initialize ritual upon paper pickup
    /// </summary>
    public void InitializeRitual(RitualPaperItem paper, SpellType spell, GameObject paperObject)
    {   
        currentPaper = paper;
        currentPaperObject = paperObject;
        
        DrawScript.Instance.SetupTexture();
        PrepareTargetTexture(spell);
    }

    Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        //Force RGBA32 format so it will always be compatible with setPixel
        Debug.Log($"ResizeTexture called: {source.width}x{source.height} -> {targetWidth}x{targetHeight}");
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);

        Debug.Log("Calculating ratios...");
        float xRatio = (float)source.width / targetWidth;
        float yRatio = (float)source.height / targetHeight;
        Debug.Log($"Ratios: x={xRatio}, y={yRatio}");

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float srcX = x * xRatio;
                float srcY = y * yRatio;
                result.SetPixel(x, y, source.GetPixelBilinear(srcX / source.width, srcY / source.height));
            }
        }

        Debug.Log("Applying texture...");
        result.Apply();
        Debug.Log("ResizeTexture DONE");
        return result;
    }

    public ComparisonResult CompareDrawing()
    {
        Texture2D drawnTexture = DrawScript.Instance.DrawImage.sprite.texture;
        return ComparePixels(drawnTexture, targetTexture);
    }

    private bool IsTextureReadable(Texture2D tex)
    {
        if (tex == null) return false;
        return tex.isReadable;           // ← this is the official property
    }

    ComparisonResult ComparePixels(Texture2D drawn, Texture2D target)
    {
        if (!IsTextureReadable(target))
        {
            Debug.Log($"Texture not readable");
            return null;
        }
        Color[] drawnPixels = drawn.GetPixels();
        Color[] targetPixels = target.GetPixels();

        int totalTargetPixels = 0;
        int matchingPixels = 0;
        int extraPixels = 0;

        float alphaThreshold = 0.1f;

        for (int i = 0; i < drawnPixels.Length; i++)
        {
            bool targetHasPixel = targetPixels[i].a > alphaThreshold;
            bool drawnHasPixel = drawnPixels[i].a > alphaThreshold;

            if (targetHasPixel)
            {
                totalTargetPixels++;
                if (drawnHasPixel)
                {
                    matchingPixels++;
                }
            }

            if (drawnHasPixel)
            {
                if (!targetHasPixel)
                {
                    // Player drew where they shouldn't have
                    extraPixels++;
                }
            }
        }

        if (totalTargetPixels == 0)
        {
            Debug.LogError("Target has no visible pixels");
            return null;
        }

        float coverage = (float)matchingPixels / totalTargetPixels;
        float extraPenalty = (float)extraPixels / totalTargetPixels;

        float accuracy = coverage - (extraPenalty * penaltyWeight);
        accuracy = Mathf.Clamp01(accuracy);

        bool success = accuracy >= requiredAccuracy;

        ComparisonResult result = new ComparisonResult
        {
            accuracy = accuracy,
            coverage = coverage,
            matchingPixels = matchingPixels,
            totalTargetPixels = totalTargetPixels,
            extraPixels = extraPixels,
            isSuccessful = success
        };

        if (showDebugInfo)
        {
            Debug.Log($"Final accuracy: {accuracy:P1}");
            Debug.Log($"Min accuracy required: {requiredAccuracy:P0}");
            Debug.Log($"Result: {(success ? "SUCCESS" : "FAILED")}");
        }

        return result;
    }

    public void CheckRitual()
    {
        ComparisonResult result = CompareDrawing();
        DrawScript.Instance.SetupTexture(); // refresh image to trace

        if (result != null)
        {
            if (result.isSuccessful)
            {
                OnRitualSuccess(result);
            }
            else
            {
                OnRitualFailed(result);
            }
        }

        // Only prepare new texture if not in paper mode
        // if (!isPaperRitual)
        // {
        //     PrepareTargetTexture(); // prepare new image
        // }
    }

    /// <summary>
    ///     Called when ritual drawing fails. 
    ///     Takes damage and returns to game, paper stays in scene for retry.
    /// </summary>
    /// <param name="result"></param>
    private void OnRitualFailed(ComparisonResult result)
    {
        Debug.Log($"<color=red>Ritual failed. Accuracy: {result.accuracy:P1} (needed {requiredAccuracy:P1})</color>");
        DrawScript.Instance.setRitualStatus(false);
        ResetPaperMode();

        PlayerHealth.Instance.TakeMentalDamage();
        UserInterface.Instance.HideInventory();
        PlayerMovement.Instance.PlayerControllerToggle(true);
    }

    /// <summary>
    ///    Called when ritual drawing succeeds. Unlocks spell and destroy paper
    /// </summary>
    /// <param name="result"></param>
    private void OnRitualSuccess(ComparisonResult result)
    {
        Debug.Log($"<color=green>Ritual complete! Accuracy: {result.accuracy:P1}</color>");
        DrawScript.Instance.setRitualStatus(false);
        if (currentPaper != null)
        {
            GameManager.Instance.UnlockSpell(currentPaper.AssignedSpell);
            Debug.Log($"Unlocked {currentPaper.AssignedSpell}");
        }

        if (currentPaperObject != null)
        {
            RitualPaperItemObject paperObject = currentPaperObject.GetComponent<RitualPaperItemObject>();
            if (paperObject != null)
                paperObject.OnRitualSuccess();
            else
                Debug.LogWarning("RitualDrawingComparison: could not find RitualPaperItemObject on paper.");
        } 

        ResetPaperMode();
        UserInterface.Instance.HideInventory();
        PlayerMovement.Instance.PlayerControllerToggle(true);
    }

    private void ResetPaperMode()
    {
        currentPaper = null;
        currentPaperObject = null;
        UserInterface.Instance.ClearItemDetails();
    }
}
