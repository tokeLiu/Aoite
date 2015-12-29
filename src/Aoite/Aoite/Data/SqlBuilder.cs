﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Aoite.Data
{
    class SqlBuilder : ISelect, IWhere
    {
        private string _fromTables;
        private List<string> _select, _orderby, _groupby;
        private IDbEngine _engine;
        private StringBuilder _whereBuilder;

        private ExecuteParameterCollection _parameters;
        public ExecuteParameterCollection Parameters
        {
            get
            {
                return this._parameters ?? (this._parameters = new ExecuteParameterCollection(0));
            }
        }

        public string Text
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("SELECT ");
                if(this._select == null) builder.Append("*");
                else this.AppendFields(builder, this._select.ToArray());

                builder.Append(" FROM ");
                builder.Append(this._fromTables);
                if(this._whereBuilder != null && this._whereBuilder.Length > 0)
                {
                    builder.Append(" WHERE ");
                    builder.Append(_whereBuilder.ToString());
                }
                if(this._groupby != null)
                {
                    builder.Append(" GROUP BY ");
                    this.AppendFields(builder, this._groupby.ToArray());
                }
                if(this._orderby != null)
                {
                    builder.Append(" ORDER BY ");
                    this.AppendFields(builder, this._orderby.ToArray());
                }
                return builder.ToString();
            }
        }

        internal SqlBuilder(IDbEngine engine)
        {
            this._engine = engine;
        }

        public ExecuteCommand End()
        {
            if(this._parameters != null) return new ExecuteCommand(this.Text, this._parameters);
            return new ExecuteCommand(this.Text);
        }

        private void AppendFields(StringBuilder builder, string[] fields)
        {
            if(fields == null || fields.Length == 0) return;
            builder.Append(fields[0]);
            for(int i = 1; i < fields.Length; i++)
            {
                builder.Append(", ");
                builder.Append(fields[i]);
            }
        }

        public ISelect Select(params string[] fields)
        {
            if(fields == null || fields.Length == 0) return this;
            if(this._select == null) this._select = new List<string>(fields.Length);
            this._select.AddRange(fields);
            return this;
        }

        public ISelect From(string fromTables)
        {
            this._fromTables = fromTables;
            return this;
        }


        public IWhere Where()
        {
            if(_whereBuilder == null) _whereBuilder = new StringBuilder();
            return this;
        }
        public IWhere Where(string expression) => this.Where().And(expression);


        public IWhere Where<T>(string fieldName, string namePrefix, T[] values) => this.Where().And(fieldName, namePrefix, values);


        public IWhere Where(string expression, string name, object value) => this.Where().And(expression, name, value);

        public IWhere WhereIn<T>(string fieldName, string namePrefix, T[] values) => this.Where().AndIn(fieldName, namePrefix, values);

        public IWhere WhereNotIn<T>(string fieldName, string namePrefix, T[] values) => this.Where().AndNotIn(fieldName, namePrefix, values);

        public ISelect OrderBy(params string[] fields)
        {
            if(fields == null || fields.Length == 0) return this;
            if(this._orderby == null) this._orderby = new List<string>(fields.Length);
            this._orderby.AddRange(fields);
            return this;
        }

        public ISelect GroupBy(params string[] fields)
        {
            if(fields == null || fields.Length == 0) return this;
            if(this._groupby == null) this._groupby = new List<string>(fields.Length);
            this._groupby.AddRange(fields);
            return this;
        }

        public IWhere Sql(string text)
        {
            this._whereBuilder.Append(text);
            return this;
        }

        public SqlBuilder Parameter(string name, object value)
        {
            this.Parameters.Add(name, value);
            return this;
        }

        ISelect ISelect.Parameter(string name, object value) => this.Parameter(name, value);
        IWhere IWhere.Parameter(string name, object value) => this.Parameter(name, value);

        public IWhere And() => this._whereBuilder.Length > 0 ? this.Sql(" AND ") : this;

        public IWhere Or() => this._whereBuilder.Length > 0 ? this.Sql(" OR ") : this;


        private SqlBuilder Append(string expression, string name, object value) => (SqlBuilder)this.Sql(expression).Parameter(name, value);

        private SqlBuilder Append<T>(string fieldName, string namePrefix, T[] values)
        {
            if(values == null || values.Length == 0) throw new ArgumentNullException(nameof(values));

            this.BeginGroup();
            int index = 0;
            fieldName = fieldName + "=";
            this.Append(fieldName + namePrefix + index, namePrefix + index, values[index]);
            for(index++; index < values.Length; index++)
            {
                this.Sql(" OR ");
                this.Append(fieldName + namePrefix + index, namePrefix + index, values[index]);
            }
            this.EndGroup();
            return this;
        }

        private SqlBuilder Append<T>(bool isNotIn, string fieldName, string namePrefix, T[] values)
        {
            if(values == null || values.Length == 0) throw new ArgumentNullException(nameof(values));
            this.Sql(fieldName)
                .Sql(isNotIn ? " NOT IN " : " IN ")
                .BeginGroup();
            int index = 0;
            this.Append(namePrefix + index, namePrefix + index, values[index]);
            for(index++; index < values.Length; index++)
            {
                this.Sql(", ");
                this.Append(namePrefix + index, namePrefix + index, values[index]);
            }
            this.EndGroup();
            return this;
        }


        public IWhere BeginGroup() => this.Sql("(");

        public IWhere BeginGroup(string expression, string name, object value)
        {
            this.BeginGroup();
            return this.Append(expression, name, value);
        }

        public IWhere EndGroup() => this.Sql(")");


        public IWhere And(string expression) => this.And().Sql(expression);

        public IWhere And(string expression, string name, object value)
        {
            this.And();
            return this.Append(expression, name, value);
        }

        public IWhere Or(string expression) => this.Or().Sql(expression);

        public IWhere Or(string expression, string name, object value)
        {
            this.Or();
            return this.Append(expression, name, value);
        }

        public IWhere And<T>(string fieldName, string namePrefix, T[] values)
        {
            this.And();
            return this.Append<T>(fieldName, namePrefix, values);
        }

        public IWhere Or<T>(string fieldName, string namePrefix, T[] values)
        {
            this.Or();
            return this.Append<T>(fieldName, namePrefix, values);
        }

        public IWhere AndIn<T>(string fieldName, string namePrefix, T[] values)
        {
            this.And();
            return this.Append(false, fieldName, namePrefix, values);
        }
        public IWhere AndNotIn<T>(string fieldName, string namePrefix, T[] values)
        {
            this.And();
            return this.Append(true, fieldName, namePrefix, values);
        }

        public IWhere OrIn<T>(string fieldName, string namePrefix, T[] values)
        {
            this.Or();
            return this.Append(false, fieldName, namePrefix, values);
        }
        public IWhere OrNotIn<T>(string fieldName, string namePrefix, T[] values)
        {
            this.Or();
            return this.Append(true, fieldName, namePrefix, values);
        }

        public IDbExecutor Execute() => this._engine.Execute(this.End());
    }
}