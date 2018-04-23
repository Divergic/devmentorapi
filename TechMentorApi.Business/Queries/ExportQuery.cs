using EnsureThat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechMentorApi.Model;

namespace TechMentorApi.Business.Queries
{
    public class ExportQuery : IExportQuery
    {
        private readonly IPhotoQuery _photoQuery;
        private readonly IProfileQuery _profileQuery;

        public ExportQuery(IProfileQuery profileQuery, IPhotoQuery photoQuery)
        {
            Ensure.Any.IsNotNull(profileQuery, nameof(profileQuery));
            Ensure.Any.IsNotNull(photoQuery, nameof(photoQuery));

            _profileQuery = profileQuery;
            _photoQuery = photoQuery;
        }

        public async Task<ExportProfile> GetExportProfile(Guid profileId, CancellationToken cancellationToken)
        {
            Ensure.Guid.IsNotEmpty(profileId, nameof(profileId));

            var profileTask = _profileQuery.GetProfile(profileId, cancellationToken);
            var photosTask = GetProfilePhotos(profileId, cancellationToken);

            await Task.WhenAll(profileTask, photosTask).ConfigureAwait(false);

            return new ExportProfile(profileTask.Result, photosTask.Result);
        }

        private async Task<IEnumerable<ExportPhoto>> GetProfilePhotos(Guid profileId, CancellationToken cancellationToken)
        {
            var photos = await _photoQuery.GetPhotos(profileId, cancellationToken).ConfigureAwait(false);

            // Return the converted photos
            return from x in photos
                   select new ExportPhoto(x);
        }
    }
}