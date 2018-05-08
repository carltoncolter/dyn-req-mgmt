var gulp = require("gulp");
var uglify = require('gulp-uglify');
var babel = require('gulp-babel');

gulp.task('uglify', function () {
    return gulp.src(['fdbzap_/**/*.js','!node_modules/**', '!fdbzap_/common/jquery*.js'])
        .pipe(babel({
            presets: ['es2015']
          }))
        .pipe(uglify())
        .pipe(gulp.dest('minifiedjs/fdbzap_'));
});