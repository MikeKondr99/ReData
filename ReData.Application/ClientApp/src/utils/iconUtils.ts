import mongodbIcon from '../assets/mongodb.png';
import mysqlIcon from '../assets/mysql.svg';
import postgresqlIcon from '../assets/postgres.png';

export const getIcon = (type: string) => {
  switch (type) {
    case 'PostgreSql':
      return postgresqlIcon;
    case 'mysql':
      return mysqlIcon;
    case 'mongodb':
      return mongodbIcon;
    default:
      return '';
  }
};
