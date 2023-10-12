using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Myriad;

public class ColorHelper<T> where T : PhysicalObject {
    public static ColorHelper<PhysicalObject> INSTANCE;

    static ColorHelper() {
        INSTANCE = new ColorHelper<PhysicalObject>();

        Type colorType = typeof(Color);

        foreach (Type inhertitedType in inhertitedTypes(typeof(PhysicalObject))) {
            List<FieldInfo> colorFieldInfos = inhertitedType.GetFields()
                .Where(info => info.FieldType == colorType).ToList();

            INSTANCE.colorGetter.Add(new Pair<Type, Func<PhysicalObject, List<Color>>>(
                inhertitedType,
                o => {
                    List<Color> colors = new List<Color>();

                    foreach (FieldInfo colorFieldInfo in colorFieldInfos) {
                        colors.Add((Color) colorFieldInfo.GetValue(o));
                    }

                    return colors;
                }));

            INSTANCE.colorSetter.Add(new Pair<Type, Func<PhysicalObject, List<Color>, int, int>>(
                inhertitedType,
                (o, list, offset) => {
                    foreach (FieldInfo colorFieldInfo in colorFieldInfos)
                    {
                        colorFieldInfo.SetValue(o, list[offset]);
                        offset++;
                    }

                    return offset;
                }));
        }
    }

    public List<Pair<Type, Func<T, List<Color>>>> colorGetter = new();
    public List<Pair<Type, Func<T, List<Color>, int, int>>> colorSetter = new();

    public List<Color> getColors(T physicalObject) {
        Type instanceType = physicalObject.GetType();

        List<Color> colors = new List<Color>();

        foreach (Pair<Type, Func<T, List<Color>>> pair in colorGetter) {
            if (instanceType.IsInstanceOfType(pair.left)) {
                colors.AddRange(pair.right(physicalObject));
            }
        }

        return colors;
    }

    public void setColors(T physicalObject, List<Color> colors) {
        Type instanceType = physicalObject.GetType();

        int offset = 0;

        foreach (Pair<Type, Func<T, List<Color>, int, int>> pair in colorSetter) {
            if (instanceType.IsInstanceOfType(pair.left)) {
                offset = pair.right(physicalObject, colors, offset);
            }
        }
    }

    public static List<Type> inhertitedTypes(Type baseType) {
        return AppDomain.CurrentDomain.GetAssemblies()
            //.Where(a => a.GetName().Name == "Assembly-CSharp")
            .SelectMany(dA => dA.GetTypes())
            .Where(type => type.IsSubclassOf(baseType))
            .ToList();
    }
}

public class Pair<L, R> {
    public L left;
    public R right;

    public Pair(L left, R right) {
        this.left = left;
        this.right = right;
    }
}