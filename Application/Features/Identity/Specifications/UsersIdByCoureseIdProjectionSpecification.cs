using Application.Specifications;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Identity.Specifications
{
    public class UsersIdByCoureseIdProjectionSpecification : BaseProjectionSpecification<UserCourse, string>
    {
        public UsersIdByCoureseIdProjectionSpecification(int courseId):base(uc => uc.ApplicationUserId.ToString())
        {
            Criteria = uc => uc.CourseId == courseId;
        }
        //public UsersIdByCoureseIdProjectionSpecification(int courseId):base(uc => uc.ApplicationUserId) => Criteria = uc => uc.CourseId == courseId;
    }
}
