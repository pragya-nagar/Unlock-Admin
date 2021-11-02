using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Response;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public class Payload<T> : IEnumerable
    {
        /// <summary>
        /// Gets or sets the Entity.
        /// </summary>
        /// <value>The Entity.</value>
        public T Entity { get; set; }

        /// <summary>
        /// Gets or sets the Entity list.
        /// </summary>
        /// <value>The Entity list.</value>
        public List<T> EntityList { get; set; }

        /// <summary>
        /// Gets or sets the paging request.
        /// </summary>
        /// <value>The paging request.</value>
        public Paging Paging { get; set; } = new Paging();

        /// <summary>
        /// Gets or sets the is success.
        /// </summary>
        /// <value>The is success.</value>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the list of message code.
        /// </summary>
        /// <value>The list of message code.</value>
        public List<string> MessageCodeList { get; set; }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// PayloadCustom
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PayloadCustom<T>
    {
        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public T Entity { get; set; }

        /// <summary>
        /// Gets or sets the view model list.
        /// </summary>
        /// <value>
        /// The view model list.
        /// </value>
        public List<T> EntityList { get; set; }

        /// <summary>
        /// Gets or sets the message list.
        /// </summary>
        /// <value>
        /// The message list.
        /// </value>
        public Dictionary<string, string> MessageList { get; set; } = new Dictionary<string, string>();


        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is success.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is success; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess { get; set; } = false;

        public int Status { get; set; }

    }

    /// <summary>
    /// PayloadCustom
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PayloadCustomGenric<T>
    {

        /// <summary>
        /// Gets or sets the view model dictionary.
        /// </summary>
        /// <value>
        /// The view model disctionary.
        /// </value>

        public Dictionary<string, string> Entity { get; set; }


        /// <summary>
        /// Gets or sets the view model list.
        /// </summary>
        /// <value>
        /// The view model list.
        /// </value>
        public List<T> EntityList { get; set; }

        /// <summary>
        /// Gets or sets the message list.
        /// </summary>
        /// <value>
        /// The message list.
        /// </value>
        public Dictionary<string, string> MessageList { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is success.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is success; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess { get; set; } = false;

        public int Status { get; set; }

        public PageInfo PaggingInfo { get; set; }
    }
    public class PayloadCustomList<T>
    {
        public T Entity { get; set; }


        /// <summary>
        /// Gets or sets the view model list.
        /// </summary>
        /// <value>
        /// The view model list.
        /// </value>
        public List<T> EntityList { get; set; }

        /// <summary>
        /// Gets or sets the message list.
        /// </summary>
        /// <value>
        /// The message list.
        /// </value>
        public Dictionary<string, string> MessageList { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is success.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is success; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess { get; set; } = false;

        public int Status { get; set; }

        public PageResults<T> PaggingInfo { get; set; }
    }
    public class PayloadCustomPassport<T>
    {
        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public T Entity { get; set; }

        /// <summary>
        /// Gets or sets the view model list.
        /// </summary>
        /// <value>
        /// The view model list.
        /// </value>
        public List<T> EntityList { get; set; }

        /// <summary>
        /// Gets or sets the message list.
        /// </summary>
        /// <value>
        /// The message list.
        /// </value>
        public List<string> MessageList { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is success.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is success; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess { get; set; } = false;
    }
}
