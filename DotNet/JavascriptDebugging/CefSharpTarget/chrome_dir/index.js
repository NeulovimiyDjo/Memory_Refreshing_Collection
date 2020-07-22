var count = 1;

var func = function () {
    var two;
    debugger;
    document.getElementById('p1').textContent = count;
    count++;
    return two + 2;
};

var result = func();

document.getElementById('p1').textContent = 'YAYAYAYA';
document.getElementById('p1').addEventListener('mouseover', func, false);