using OSGI_Datatypes.OrganisationElements;
using System;


namespace OSGI_Datatypes.ComposedTypes
{
    public class ContainingInfo: IComparable
    {
        public Commit first;
        public Commit last;
        public EndReason endReason;

        public ContainingInfo(Commit f, Commit l, EndReason er)
        {
            first = f;
            last = l;
            endReason = er;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is ContainingInfo)
            {
                ContainingInfo other = (ContainingInfo)obj;

                int firstCompRes = first.CompareTo(other.first);
                if (firstCompRes != 0)
                {
                    return firstCompRes;
                }
                if(last==null && other.last == null)
                {
                    return 0;
                }
                else if(last == null)
                {
                    return 1;
                }
                else if(other.last == null)
                {
                    return -1;
                }
                else
                {
                    return -1 * last.CompareTo(other.last);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

    }
}
