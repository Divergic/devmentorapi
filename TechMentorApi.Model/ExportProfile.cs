using EnsureThat;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TechMentorApi.Model
{
    public class ExportProfile : Profile
    {
        public ExportProfile()
        {
            Photos = new Collection<ExportPhoto>();
        }

        public ExportProfile(Profile profile, IEnumerable<ExportPhoto> photos) : base(profile)
        {
            Ensure.Any.IsNotNull(profile, nameof(profile));
            Ensure.Any.IsNotNull(photos, nameof(photos));

            Id = profile.Id;
            BannedAt = profile.BannedAt;

            Photos = new Collection<ExportPhoto>(photos.ToList());
        }

        public Collection<ExportPhoto> Photos { get; }
    }
}