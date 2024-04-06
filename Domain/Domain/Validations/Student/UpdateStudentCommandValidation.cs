using Domain.Commands.Student;

namespace Domain.Validations
{
    public class UpdateStudentCommandValidation : StudentValidation<UpdateStudentCommand>
    {
        public UpdateStudentCommandValidation()
        {
            ValidateId();
            ValidateName();
            ValidateBirthDate();
            ValidateEmail();
            ValidatePhone();
        }
    }
}