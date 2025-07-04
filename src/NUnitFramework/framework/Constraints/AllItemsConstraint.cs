// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// AllItemsConstraint applies another constraint to each
    /// item in a collection, succeeding if they all succeed.
    /// </summary>
    public class AllItemsConstraint : PrefixConstraint, IEnumerableConstraint
    {
        /// <summary>
        /// Construct an AllItemsConstraint on top of an existing constraint
        /// </summary>
        /// <param name="itemConstraint"></param>
        public AllItemsConstraint(IConstraint itemConstraint)
            : base(itemConstraint, "all items")
        {
        }

        /// <summary>
        /// The display name of this Constraint for use by ToString().
        /// The default value is the name of the constraint with
        /// trailing "Constraint" removed. Derived classes may set
        /// this to another name in their constructors.
        /// </summary>
        public override string DisplayName => "All";

        /// <summary>
        /// Apply the item constraint to each item in the collection,
        /// failing if any item fails.
        /// </summary>
        /// <inheritdoc cref="IConstraint.ApplyTo{TActual}(TActual)"/>
        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            var enumerable = ConstraintUtils.RequireActual<IEnumerable>(actual, nameof(actual));
            var itemType = TypeHelper.FindPrimaryEnumerableInterfaceGenericTypeArgument(typeof(TActual));

            return itemType is null || itemType == typeof(object)
                ? ApplyToEnumerable(actual, enumerable.Cast<object>())
                : Reflect.InvokeApplyToEnumerable(this, actual, itemType);
        }

        /// <summary>
        /// Apply the item constraint to each item in the collection,
        /// failing if any item fails.
        /// </summary>
        /// <inheritdoc cref="IEnumerableConstraint.ApplyToEnumerable{TActual, TItem}(TActual, IEnumerable{TItem})"/>
        public ConstraintResult ApplyToEnumerable<TActual, TItem>(TActual actual, IEnumerable<TItem> enumerable)
        {
            int index = 0;
            foreach (var item in enumerable)
            {
                if (!BaseConstraint.ApplyTo(item).IsSuccess)
                {
                    return new EachItemConstraintResult(this, actual, item, index);
                }

                index++;
            }

            return new ConstraintResult(this, actual, ConstraintStatus.Success);
        }
    }
}
