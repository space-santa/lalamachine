const functions = require('../functions');

test('1 to be 1', () => {
  expect(functions.getSafeValue(1)).toBe(1);
});
test('string to be string', () => {
  expect(functions.getSafeValue("la")).toBe("la");
});
test('undefined to be empty string', () => {
  var lala;
  expect(functions.getSafeValue(lala)).toBe("");
});