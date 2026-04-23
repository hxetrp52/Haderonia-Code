using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Stat: 단일 스탯을 표현합니다. BaseValue와 modifier 목록을 가지며
// Value는 modifier 적용 후 최종값을 반환합니다.
public class Stat
{
    public float BaseValue { get; private set; }
    private readonly List<StatModifier> modifiers = new List<StatModifier>();

    public event Action<float> OnValueChanged;

    public Stat(float baseValue = 0f)
    {
        BaseValue = baseValue;
    }

    public float Value
    {
        get
        {
            // 적용 순서: 합산(Add/Subtract) -> 곱셈/나눗셈(Multiply/Divide)
            float result = BaseValue;

            // 합산 계열 처리
            float add = modifiers.Where(m => m.Operation == StatOperation.Add).Sum(m => m.Value * m.Stack);
            float sub = modifiers.Where(m => m.Operation == StatOperation.Subtract).Sum(m => m.Value * m.Stack);

            result += add;
            result -= sub;

            // 곱셈/나눗셈 처리
            float mul = 1f;
            foreach (var m in modifiers.Where(x => x.Operation == StatOperation.Multiply))
                mul *= Mathf.Clamp(m.Value, -10000f, 10000f);

            float div = 1f;
            foreach (var m in modifiers.Where(x => x.Operation == StatOperation.Divide))
                div *= Mathf.Approximately(m.Value, 0f) ? 1f : m.Value;

            result = (result * mul) / (div == 0f ? 1f : div);

            return result;
        }
    }

    // 베이스값 설정
    public void SetBaseValue(float value)
    {
        BaseValue = value;
        OnValueChanged?.Invoke(Value);
    }

    // modifier 추가 (기존 Id가 있으면 스택 증가)
    public void AddModifier(StatModifier modifier)
    {
        var existing = modifiers.Find(m => m.Id == modifier.Id && m.Operation == modifier.Operation && Mathf.Approximately(m.Value, modifier.Value));
        if (existing != null)
        {
            existing.AddStack(modifier.Stack);
        }
        else
        {
            modifiers.Add(modifier);
        }
        OnValueChanged?.Invoke(Value);
    }

    // Id로 modifier 제거
    public void RemoveModifierById(string id)
    {
        modifiers.RemoveAll(m => m.Id == id);
        OnValueChanged?.Invoke(Value);
    }
}
