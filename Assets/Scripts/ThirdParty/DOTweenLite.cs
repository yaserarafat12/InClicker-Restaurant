using System;
using UnityEngine;
using UnityEngine.UI;

namespace DG.Tweening
{
    public enum Ease { Linear, OutBack, InBack, OutQuad }
    public enum LoopType { Restart, Yoyo }

    public class Tweener
    {
        public Tweener SetEase(Ease ease) => this;
        public Tweener SetLoops(int loops, LoopType loopType = LoopType.Restart) => this;
        public Tweener OnComplete(Action callback)
        {
            callback?.Invoke();
            return this;
        }
    }

    public static class DOTween
    {
        public static Tweener To(Func<float> getter, Action<float> setter, float endValue, float duration)
        {
            setter?.Invoke(endValue);
            return new Tweener();
        }
    }

    public static class DOVirtual
    {
        public static Tweener DelayedCall(float delay, Action callback)
        {
            callback?.Invoke();
            return new Tweener();
        }
    }

    public static class DOTweenLiteExtensions
    {
        public static Tweener DOFade(this CanvasGroup target, float endValue, float duration)
        {
            if (target != null) target.alpha = endValue;
            return new Tweener();
        }

        public static Tweener DOFade(this Graphic target, float endValue, float duration)
        {
            if (target != null)
            {
                var color = target.color;
                color.a = endValue;
                target.color = color;
            }
            return new Tweener();
        }

        public static Tweener DOColor(this Graphic target, Color endValue, float duration)
        {
            if (target != null) target.color = endValue;
            return new Tweener();
        }

        public static Tweener DOScale(this Transform target, Vector3 endValue, float duration)
        {
            if (target != null) target.localScale = endValue;
            return new Tweener();
        }

        public static Tweener DOPunchScale(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
        {
            return new Tweener();
        }

        public static Tweener DOPunchPosition(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
        {
            return new Tweener();
        }

        public static Tweener DOLocalMoveX(this Transform target, float endValue, float duration)
        {
            if (target != null)
            {
                var pos = target.localPosition;
                pos.x = endValue;
                target.localPosition = pos;
            }
            return new Tweener();
        }

        public static Tweener DOLocalMoveY(this Transform target, float endValue, float duration)
        {
            if (target != null)
            {
                var pos = target.localPosition;
                pos.y = endValue;
                target.localPosition = pos;
            }
            return new Tweener();
        }

        public static Tweener DOShakeAnchorPos(this RectTransform target, float duration, float strength = 100f, int vibrato = 10, float randomness = 90f)
        {
            return new Tweener();
        }

        public static Tweener DOAnchorPos(this RectTransform target, Vector2 endValue, float duration)
        {
            if (target != null) target.anchoredPosition = endValue;
            return new Tweener();
        }

        public static Tweener DOPunchAnchorPos(this RectTransform target, Vector2 punch, float duration, int vibrato = 10, float elasticity = 1f)
        {
            return new Tweener();
        }

        public static Tweener DOText(this Text target, string endValue, float duration)
        {
            if (target != null) target.text = endValue;
            return new Tweener();
        }

        public static void DOKill(this Component target) { }
    }
}
