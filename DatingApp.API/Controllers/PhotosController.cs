using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers {
    [Authorize]
    [Route ("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController (IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig) {
            _cloudinaryConfig = cloudinaryConfig;
            _repo = repo;
            _mapper = mapper;

            Account account = new Account (
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );
            _cloudinary = new Cloudinary (account);
        }

        [HttpGet ("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto (int id) {
            var photofromrepo = await _repo.GetPhoto (id);
            var photo = _mapper.Map<PhotoToReturnDto> (photofromrepo);
            return Ok (photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser (int userId, PhotoToCreateDto photoToCreate) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            } else {
                var userFromRepo = await _repo.GetUser (userId);
                var file = photoToCreate.File;
                var uploadResult = new ImageUploadResult ();
                if (file.Length > 0) {
                    using (var stream = file.OpenReadStream ()) {
                        var uploadParams = new ImageUploadParams () {
                        File = new FileDescription (file.Name, stream),
                        Transformation = new Transformation ().Width (500).Height (500).Crop ("fill").Gravity ("face")
                        };
                        uploadResult = _cloudinary.Upload (uploadParams);
                    }
                }
                photoToCreate.Url = uploadResult.Uri.ToString ();
                photoToCreate.PublicId = uploadResult.PublicId;

                var photo = _mapper.Map<Photo> (photoToCreate);
                if (!userFromRepo.Photos.Any (x => x.IsMain)) {
                    photo.IsMain = true;
                }
                userFromRepo.Photos.Add (photo);
                if (await _repo.SaveAll ()) {
                    var photoToReturn = _mapper.Map<PhotoToReturnDto> (photo);
                    return CreatedAtRoute ("GetPhoto", new { userId = userId, id = photo.Id }, photoToReturn);
                }
                return BadRequest ("Error uploading photo");
            }
        }
    }
}