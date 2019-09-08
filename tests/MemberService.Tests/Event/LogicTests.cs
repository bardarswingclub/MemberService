using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemberService.Pages.Event;
using NUnit.Framework;
using Shouldly;

namespace MemberService.Tests.Event
{
    [TestFixture]
    public class LogicTests
    {
        [Test]
        public void SetEventStatus_Open()
        {
            using (TemporaryTime.Is(DateTime.UtcNow))
            {
                var model = new Data.Event
                {
                    SignupOptions =
                    {
                        SignupClosesAt = TimeProvider.UtcNow
                    }
                };

                model.SetEventStatus("open");

                model.SignupOptions.SignupOpensAt.ShouldBe(TimeProvider.UtcNow);
                model.SignupOptions.SignupClosesAt.ShouldBeNull();
                model.Archived.ShouldBeFalse();
            }
        }

        [Test]
        public void SetEventStatus_Close()
        {
            using (TemporaryTime.Is(DateTime.UtcNow))
            {
                var model = new Data.Event
                {
                    SignupOptions =
                    {
                        SignupOpensAt = TimeProvider.UtcNow.AddDays(-1)
                    }
                };

                model.SetEventStatus("close");

                model.SignupOptions.SignupOpensAt.ShouldBe(TimeProvider.UtcNow.AddDays(-1));
                model.SignupOptions.SignupClosesAt.ShouldBe(TimeProvider.UtcNow);
                model.Archived.ShouldBeFalse();
            }
        }

        [Test]
        public void SetEventStatus_Archive()
        {
            using (TemporaryTime.Is(DateTime.UtcNow))
            {
                var model = new Data.Event
                {
                    SignupOptions =
                    {
                        SignupOpensAt = TimeProvider.UtcNow.AddDays(-1)
                    }
                };

                model.SetEventStatus("archive");

                model.SignupOptions.SignupOpensAt.ShouldBe(TimeProvider.UtcNow.AddDays(-1));
                model.SignupOptions.SignupClosesAt.ShouldBe(TimeProvider.UtcNow);
                model.Archived.ShouldBeTrue();
            }
        }

        [Test]
        public void SetEventStatus_ArchiveAlreadyClosed()
        {
            using (TemporaryTime.Is(DateTime.UtcNow))
            {
                var model = new Data.Event
                {
                    SignupOptions =
                    {
                        SignupOpensAt = TimeProvider.UtcNow.AddDays(-2),
                        SignupClosesAt = TimeProvider.UtcNow.AddDays(-1)
                    }
                };

                model.SetEventStatus("archive");

                model.SignupOptions.SignupOpensAt.ShouldBe(TimeProvider.UtcNow.AddDays(-2));
                model.SignupOptions.SignupClosesAt.ShouldBe(TimeProvider.UtcNow.AddDays(-1));
                model.Archived.ShouldBeTrue();
            }
        }
    }
}
