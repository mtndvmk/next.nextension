namespace Nextension
{
    public class NFoldableAttribute : ApplyToCollectionPropertyAttribute
    {
        public NFoldableAttribute()
        {
            this.order = NAttributeOrder.FOLDABLE;
        }
    }
}
