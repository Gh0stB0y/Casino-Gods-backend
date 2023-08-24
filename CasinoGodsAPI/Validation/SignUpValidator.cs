using CasinoGodsAPI.Data;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Models.DatabaseModels;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CasinoGodsAPI.Validation
{
    public class SignUpValidator : AbstractValidator<SignUpDTO>
    {
        private CasinoGodsDbContext _casinoGodsDbContext;

        public SignUpValidator(CasinoGodsDbContext casinoGodsDbContext)
        {
            _casinoGodsDbContext= casinoGodsDbContext;

            RuleFor(p => p.Username).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("{PropertyName} is empty")
                .Length(3, 50).WithMessage("Length of {PropertyName} must be between 3 and 50 characters")
                .MustAsync(async (p,cancellationToken) => await UsernameNotTaken(p)).WithMessage("{PropertyName} is already taken");
            RuleFor(p => p.Email).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("{PropertyName} is empty")
                .EmailAddress().WithMessage("{PropertyName} is not valid")
                .MustAsync(async (p, cancellationToken) => await EmailNotTaken(p)).WithMessage("{PropertyName} is already taken");
            RuleFor(p => p.Birthdate).Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty().WithMessage("{PropertyName} is empty")
                .Must(AgeGood).WithMessage("User is not an adult");
            RuleFor(p => p.Password).Cascade(CascadeMode.StopOnFirstFailure)
                .MinimumLength(8).WithMessage("{PropertyName} is too short")
                .Must(SpecialLetterGood).WithMessage("{PropertyName} does not contain a special character")
                .Must(PassNumGood).WithMessage("{PropertyName} does not contain a digit")
                .Must(CheckLowecase).WithMessage("{PropertyName} does not contain lowercase letter")
                .Must(CheckUppercase).WithMessage("{PropertyName} does not contain upercase letter");
        }

        private async Task<bool> UsernameNotTaken(string username)
        {
            var searchUsername = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Username == username);
            return searchUsername == null;

        }
        private async Task<bool> EmailNotTaken(string email)
        {
            var searchEmail = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Email == email);
            return searchEmail == null;
        }
        private bool AgeGood(DateTime birthDate)
        {
            if (birthDate.Year + 18 < DateTime.Today.Year) return true;
            else if (birthDate.Year + 18 == DateTime.Today.Year)
            {
                if (birthDate.Month < DateTime.Today.Month) return true;
                else if (birthDate.Month == DateTime.Today.Month)
                {
                    if (birthDate.Day <= DateTime.Today.Day) return true;
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        private static bool PassNumGood(string password)
        {

            /*int res;
            bool numExist = false;
            foreach (char character in password)
            {
                //if (int.TryParse(character.ToString(), out res)) { numExist = true; break; }
                if (char.IsDigit(character)) { numExist = true; break; }
            }
            return numExist;*/
            return password.Any(ch => char.IsDigit(ch));
        }
        private static bool SpecialLetterGood(string password)
        {
            return password.Any(ch => !char.IsLetterOrDigit(ch));
        }
        private static bool CheckUppercase(string password)
        {
            /*bool upperExist = false;
            foreach (char character in password)
            {
                if (char.IsUpper(character)) { upperExist = true; break; }
            }
            return upperExist;*/
            return password.Any(ch => char.IsUpper(ch));
        }
        private static bool CheckLowecase(string password)
        {
            /*bool lowerExist = false;
            foreach (char character in password)
            {
                if (char.IsLower(character)) { lowerExist = true; break; }
            }
            return lowerExist;*/
            return password.Any(ch => char.IsLower(ch));
        }
    }
}
