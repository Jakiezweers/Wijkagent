using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WijkAgent2.Classes
{
    class Comment
    {
        public string CommentPoster { get; set; }
        public DateTime CommentDate { get; set; }
        public string CommentText { get; set; }

        public Comment()
        {

        }

        public override String ToString()
        {
            return $"{CommentText} written by {CommentPoster} date: {CommentDate}";
        }
    }
}
