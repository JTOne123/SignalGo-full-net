﻿using SignalGo.DataExchanger.Conditions;
using SignalGo.DataExchanger.Helpers;
using System;
using System.Collections.Generic;

namespace SignalGo.DataExchanger.Compilers
{
    /// <summary>
    /// compile the conditions and wheres
    /// </summary>
    public class ConditionsCompiler
    {
        /// <summary>
        /// all of supported methods like 'count' 'sum' etc
        /// </summary>
        private Dictionary<string, IRunnable> SupportedMethods { get; set; } = new Dictionary<string, IRunnable>();
        /// <summary>
        /// base variable on compiler 
        /// </summary>
        public VariableInfo VariableInfo { get; set; } = null;

        public void Compile(string query)
        {
            string selectQuery = query.Trim();
            //find first select word in the query we find this and then get select query from back
            int indexOfFirstSelect = selectQuery.IndexOf("var", StringComparison.OrdinalIgnoreCase);
            if (indexOfFirstSelect == 0)
            {
                selectQuery = query;
            }
            else
                return;
            int index = 0;
            VariableInfo = new VariableInfo()
            {
                WhereInfo = new WhereInfo()
            };
            VariableInfo.WhereInfo.PublicVariables = VariableInfo.PublicVariables;
            ExtractVariables(selectQuery, ref index, VariableInfo.WhereInfo);
        }

        public object Run(object obj)
        {
            return VariableInfo.Run(obj);
        }

        /// <summary>
        /// extract full 'var'
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <param name="index"></param>
        /// <param name="parent"></param>
        private void ExtractVariables(string selectQuery, ref int index, IAddConditionSides parent)
        {
            //find variable with 'var' word
            byte foundVariableStep = 0;
            bool canSkip = true;
            //variable name that found after 'var'
            string variableName = "";
            string concat = "";
            //calculate all of char after select query
            for (int i = index; i < selectQuery.Length; i++)
            {
                //current char
                char currentChar = selectQuery[i];
                bool isWhiteSpaceChar = StringHelper.IsWhiteSpaceCharacter(currentChar);
                if (canSkip)
                {
                    //skip white space chars
                    if (isWhiteSpaceChar)
                        continue;
                    else
                        canSkip = false;
                }
                concat += currentChar;
                if (foundVariableStep == 2)
                {
                    if (string.IsNullOrEmpty(variableName) && string.IsNullOrEmpty(concat))
                        throw new Exception("I cannot name of variable before '{' char");
                    //breack char
                    if (StringHelper.IsWhiteSpaceCharacter(currentChar) || currentChar == '{')
                    {
                        variableName = concat.Trim('{').Trim();
                        VariableInfo.PublicVariables.Add(variableName, null);
                        concat = "";
                        index = i + 1;
                        ExtractWhere(selectQuery, ref index, parent);
                        break;
                    }
                }
                else if (foundVariableStep == 1)
                {
                    if (concat.Equals("in", StringComparison.OrdinalIgnoreCase))
                    {
                        concat = "";
                        foundVariableStep = 2;
                        canSkip = true;
                    }
                    else if (concat.Length >= 2)
                        throw new Exception($"I cannot find 'In' after variable name, are you sure don't forgot to write 'In' ? I found {concat}");
                }
                else if (concat.Equals("var", StringComparison.OrdinalIgnoreCase))
                {
                    concat = "";
                    canSkip = true;
                    //if that is first 'var' skip step 1 to find 'in' example (var x in user.posts) this is not first variable 
                    if (parent == VariableInfo.WhereInfo)
                        foundVariableStep = 2;
                    else
                        foundVariableStep = 1;
                }
            }

        }

        /// <summary>
        /// extract full 'where'
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <param name="index"></param>
        /// <param name="parent"></param>
        private void ExtractWhere(string selectQuery, ref int index, IAddConditionSides parent)
        {
            //find variable with 'var' word
            byte foundVariableStep = 0;
            bool canSkip = true;
            //variable name that found after 'var'
            string variableName = "";
            string concat = "";
            //calculate all of char after select query
            for (int i = index; i < selectQuery.Length; i++)
            {
                //current char
                char currentChar = selectQuery[i];
                bool isWhiteSpaceChar = StringHelper.IsWhiteSpaceCharacter(currentChar);
                if (canSkip)
                {
                    //skip white space chars
                    if (isWhiteSpaceChar || (foundVariableStep == 0 && currentChar == '{'))
                        continue;
                    else
                        canSkip = false;
                }
                concat += currentChar;
                if (currentChar == '}')
                    break;
                //find left side
                if (foundVariableStep == 1)
                {
                    if (currentChar == '"')
                    {
                        index = i;
                        ExtractString(selectQuery, ref index, parent);
                        i = index;
                    }
                    else
                        CheckVariable(currentChar);
                }
                //find operator
                else if (foundVariableStep == 2)
                {
                    if (OperatorInfo.SupportedOperators.TryGetValue(concat.ToLower(), out OperatorType currentOperator))
                    {
                        parent.ChangeOperatorType(currentOperator);
                        //OperatorKey findEmpty = parent.WhereInfo.Operators.FirstOrDefault(x => x.OperatorType == OperatorType.None);
                        foundVariableStep = 1;
                        concat = "";
                        canSkip = true;
                    }
                    else if (concat.Length > 3)
                        throw new Exception($"I cannot found operator,I try but found '{concat}' are you sure you don't missed?");
                }
                else if (concat.Equals("where", StringComparison.OrdinalIgnoreCase))
                {
                    concat = "";
                    canSkip = true;
                    foundVariableStep = 1;
                }
            }

            void CheckVariable(char currentChar)
            {
                string trim = concat.Trim();
                //step 1 of left side complete
                if (trim.Length > 0)
                {
                    if (StringHelper.IsWhiteSpaceCharacter(currentChar))
                    {
                        variableName = trim;
                        concat = "";
                        canSkip = true;
                        if (foundVariableStep == 1)
                        {
                            //calculate left side
                            CalculateSide(variableName, parent);
                            foundVariableStep++;
                        }
                    }
                    //variable is method
                    //that is method so find method name and data
                    else if (currentChar == '(')
                    {
                        //method name
                        variableName = trim.Trim('(').ToLower();
                        if (!SupportedMethods.ContainsKey(variableName))
                            throw new Exception($"I cannot find method '{variableName}' are you sure you wrote true?");
                    }
                }
            }
        }

        private void ExtractString(string selectQuery, ref int index, IAddConditionSides parent)
        {
            bool isStart = false;
            bool isAfterQuote = false;
            string concat = "";
            //calculate all of char after select query
            int i = index;
            for (; i < selectQuery.Length; i++)
            {
                //current char
                char currentChar = selectQuery[i];
                if (isStart)
                {
                    if (currentChar == '"')
                    {
                        //before this was '"'
                        if (isAfterQuote)
                        {
                            isAfterQuote = false;
                            concat += currentChar;
                        }
                        //is '"' after '"'
                        else if (selectQuery[i + 1] == '"')
                        {
                            isAfterQuote = true;
                            concat += currentChar;
                        }
                        else
                            break;
                    }
                    else
                    {
                        concat += currentChar;
                    }
                }
                else if (currentChar == '"')
                {
                    isStart = true;
                }
            }
            index = i;
            CalculateSide(concat, parent, true);
        }
        /// <summary>
        /// after finished to read a side need to calculate
        /// sides are left and right of an operator like '=' '>' '>=' etc
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parent"></param>
        /// <param name="isString"></param>
        private void CalculateSide(string data, IAddConditionSides parent, bool isString = false)
        {
            IRunnable sideValue = null;
            if (isString)
                sideValue = new ValueInfo() { Value = data, PublicVariables = parent.PublicVariables };
            else
            {
                sideValue = new PropertyInfo() { PropertyPath = data, PublicVariables = parent.PublicVariables };
            }
            parent.Add(sideValue);
        }
    }
}