using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceManager : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private Slider _experienceSlider;
    [SerializeField] private TMP_Text _levelText;
    [Header("Experience")]
    [Min(1)]
    [SerializeField] private int _weight = 5;
    [Min(0)]
    [SerializeField] private int _startingLevel = 0;
    [Min(0)]
    [SerializeField] private int _startingExperience = 0;

    public static ExperienceManager Instance { get; private set; }

    private int _currentExperience;
    private int _currentLevel = 0;

    public int CurrentExperience => _currentExperience;
    public int CurrentLevel => _currentLevel;
    public int RequiredExperienceForNextLevel => GetRequiredExperienceForLevel(_currentLevel + 1);

    /// <summary>Fired whenever XP changes. (currentXP, requiredForNextLevel)</summary>
    public event Action<int, int> ExperienceChanged;
    /// <summary>Fired whenever level changes. (newLevel)</summary>
    public event Action<int> LevelChanged;
    /// <summary>Fired whenever you level up. (newLevel)</summary>
    public event Action<int> LevelUp;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        // Initialize from inspector values by default.
        _currentLevel = Mathf.Max(0, _startingLevel);
        _currentExperience = Mathf.Max(0, _startingExperience);
    }

    private void Start()
    {
        UpdateUI();
    }

    private void OnValidate()
    {
        _weight = Mathf.Max(1, _weight);
        _startingLevel = Mathf.Max(0, _startingLevel);
        _startingExperience = Mathf.Max(0, _startingExperience);

        _currentLevel = Mathf.Max(0, _currentLevel);
        _currentExperience = Mathf.Max(0, _currentExperience);

        // Keep UI responsive in-editor.
        if (isActiveAndEnabled)
            UpdateUI();
    }

    private int GetRequiredExperienceForLevel(int targetLevel)
    {
        int lvl = Mathf.Max(0, targetLevel);
        int w = Mathf.Max(1, _weight);

        // Keep your original formula, but guarantee it's always >= 1 to avoid infinite loops.
        int required = (int)(lvl * Mathf.Pow(w, 2));
        return Mathf.Max(1, required);
    }


    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        _currentExperience += amount;
        ProcessLevelUpsAndRefresh();
    }

    public void SetProgress(int level, int experience)
    {
        _currentLevel = Mathf.Max(0, level);
        _currentExperience = Mathf.Max(0, experience);
        ProcessLevelUpsAndRefresh();
    }

    private void ProcessLevelUpsAndRefresh()
    {
        // Handle overflow XP (e.g., gaining enough to skip multiple levels).
        int safety = 0;
        while (_currentExperience >= RequiredExperienceForNextLevel && safety++ < 1000)
        {
            _currentExperience -= RequiredExperienceForNextLevel;
            _currentLevel++;
            LevelChanged?.Invoke(_currentLevel);
            LevelUp?.Invoke(_currentLevel);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        int required = RequiredExperienceForNextLevel;

        if (_experienceSlider != null)
        {
            float normalized = required <= 0 ? 0f : Mathf.Clamp01((float)_currentExperience / required);
            _experienceSlider.minValue = 0f;
            _experienceSlider.maxValue = 1f;
            _experienceSlider.SetValueWithoutNotify(normalized);
        }

        if (_levelText != null)
            _levelText.text = $"Level: {_currentLevel}";

        ExperienceChanged?.Invoke(_currentExperience, required);
    }
}