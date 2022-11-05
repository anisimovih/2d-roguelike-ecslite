using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Roguelike.Features.Audio;
using Roguelike.Features.Sprite;

namespace Roguelike.Features.Health
{
    public class HealthSystem : IEcsRunSystem
    {
        private readonly EcsFilterInject<Inc<HealthChangeEventComponent>> _healthChangedEventFilter = default;

        private readonly EcsPoolInject<HealthChangeEventComponent> _healthChangePool = default;
        private readonly EcsPoolInject<HealthComponent> _healthPool = default;
        
        private readonly EcsPoolInject<SpriteChangeEventComponent> _changeSpritePool = default;
        private readonly EcsPoolInject<SpriteResourceComponent> _spriteResourcePool = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var e in _healthChangedEventFilter.Value)
            {
                var healthChange = _healthChangePool.Value.Get(e);
                ref var health = ref _healthPool.Value.Get(e);
                health.CurrentHealth += healthChange.HealthChangeAmount;

                if (health.IsDamaged)
                    TryChangeSpriteToDamaged(e);
            }
        }

        private void TryChangeSpriteToDamaged(int entity)
        {
            var spriteResourcePool = _spriteResourcePool.Value;
            if (!spriteResourcePool.Has(entity)) return;
            
            ref var spriteResource = ref spriteResourcePool.Get(entity);
            spriteResource.TypeToSprite.TryGetValue(SpriteType.DAMAGED, out var sprite);
            if (sprite == null) return;
            
            ref var changeSprite = ref _changeSpritePool.Value.Add(entity);
            changeSprite.sprite = sprite;
        }
    }
}