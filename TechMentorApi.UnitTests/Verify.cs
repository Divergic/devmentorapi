namespace TechMentorApi.UnitTests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using FluentAssertions.Execution;
    using NSubstitute.Core;
    using NSubstitute.Core.Arguments;

    /// <summary>
    ///     The <see cref="Verify" />
    ///     class is used to provide FluentAssertion evaluations for NSubstitute arguments.
    /// </summary>
    public static class Verify
    {
        /// <summary>
        ///     The argument specification queue.
        /// </summary>
        private static readonly ArgumentSpecificationQueue _queue =
            new ArgumentSpecificationQueue(SubstitutionContext.Current);

        /// <summary>
        /// Determines whether the specified action passes assertion evaluation.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <returns>
        /// <c>true</c> if there are no assertion failures; otherwise <c>false</c>.
        /// </returns>
        public static bool Is(Action action)
        {
            using (var scope = new AssertionScope())
            {
                action();

                var failures = scope.Discard().ToList();

                if (failures.Count == 0)
                {
                    return true;
                }

                failures.ForEach(x => Trace.WriteLine(x));

                return false;
            }
        }

        /// <summary>
        /// Runs an action that determines whether.
        /// </summary>
        /// <typeparam name="T">
        /// The type of value being evaluated.
        /// </typeparam>
        /// <param name="action">
        /// The action that evaluates the value.
        /// </param>
        /// <returns>
        /// The evaluated value.
        /// </returns>
        public static T That<T>(Action<T> action)
        {
            return _queue.EnqueueSpecFor<T>(new AssertionMatcher<T>(action));
        }

        /// <summary>
        /// The <see cref="AssertionMatcher{T}"/>
        ///     class is used to run an action in a FluentAssertions scope to determine a predicate result.
        /// </summary>
        /// <typeparam name="T">
        /// The type of value being asserted.
        /// </typeparam>
        private class AssertionMatcher<T> : IArgumentMatcher
        {
            /// <summary>
            ///     The assertion action.
            /// </summary>
            private readonly Action<T> _assertion;

            /// <summary>
            /// Initializes a new instance of the <see cref="AssertionMatcher{T}"/> class.
            /// </summary>
            /// <param name="assertion">
            /// The assertion.
            /// </param>
            public AssertionMatcher(Action<T> assertion)
            {
                _assertion = assertion;
            }

            /// <inheritdoc />
            public bool IsSatisfiedBy(object argument)
            {
                using (var scope = new AssertionScope())
                {
                    _assertion((T)argument);

                    var failures = scope.Discard().ToList();

                    if (failures.Count == 0)
                    {
                        return true;
                    }

                    failures.ForEach(x => Trace.WriteLine(x));

                    return false;
                }
            }
        }
    }
}