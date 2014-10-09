
Ext.apply(Ext.form.VTypes,
{
  sourceAndDestination: function (value, field) {

    var otherField = Ext.getCmp(field.otherField);

    var otherValue = otherField.value;
    return (otherValue != field.value);
  },
  sourceAndDestinationText: "The destination cannot be the same as the source",
  otherIsNotNull: function (value, field) {
    var otherField = Ext.getCmp(field.otherField);
    this.otherIsNotNullText = "The " + otherField.initialConfig.fieldLabel + " field cannot be empty";
    var value = otherField.value;

    return (value != undefined && value != null && value != '');
  },
  otherIsNotNullText: "The other field cannot be empty.",

  priceRoundingFormula: function (value, field) {

    var expressionConstraint = '';
    var expressionReverseConstraint = '';
    var expression = expressionConstraint + '|' + expressionReverseConstraint;


    var ex = new RegExp('^(((\\s*(\\*)*(,){0,1}[0-9]+\\s*(<=|==|>=|>|<|<>)\\s*(x|m))|' +
'((x|m)\\s*(<=|==|>=|>|<|<>)\\s*(\\*)*(,){0,1}[0-9]+))\\s*(\\s*(&&|\\|\\|)\\s*((\\s*(\\*)*(,){0,1}[0-9]+\\s*(<=|==|>=|>|<|<>)\\s*(x|m))|' +
'((x|m)\\s*(<=|==|>=|>|<|<>)\\s*(\\*)*(,){0,1}[0-9]+))\\s*)*' +
'(=\\s*(,){1}[0-9]+))$');

    return ex.test(value);
  },

  priceRoundingFormulaText: 'The formula is not valid. Refer to help',
  daterange : function(val, field) {
        var date = field.parseDate(val);

        if(!date){
            return false;
        }
        if (field.startDateField) {
            var start = Ext.getCmp(field.startDateField);
            if (!start.maxValue || (date.getTime() != start.maxValue.getTime())) {
                start.setMaxValue(date);
                start.validate();
            }
        }
        else if (field.endDateField) {
            var end = Ext.getCmp(field.endDateField);
            if (!end.minValue || (date.getTime() != end.minValue.getTime())) {
                end.setMinValue(date);
                end.validate();
            }
        }
        /*
         * Always return true since we're only using this vtype to set the
         * min/max allowed values (these are tested for after the vtype test)
         */
        return true;
    }
}
);