// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using Octgn.DataNew.Entities;

namespace Octide
{
    public class RichTextViewModel : ViewModelBase
    {
        private RichTextPropertyValue _value;

        public RichTextPropertyValue Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value) return;
                _value = value;
                RaisePropertyChanged("Value");
            }

        }
    }
}
