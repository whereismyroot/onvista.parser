TRUNCATE TABLE `articles`;
ALTER TABLE `articles` 
ADD COLUMN `relative_url` VARCHAR(255) NOT NULL AFTER `title`,
ADD UNIQUE INDEX `relative_url_UNIQUE` (`relative_url` ASC),
DROP INDEX `title_UNIQUE` ;