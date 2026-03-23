const isDocker = window.location.hostname === 'localhost' && window.location.port === '8080';

export const ImagePaths = {
  portfolio: isDocker ? '/images/portfolio/' : '/assets/images/portfolio/',
  fillings: isDocker ? '/images/fillings/' : '/assets/images/fillings/',
  placeholder: isDocker ? '/images/placeholder-cake.jpg' : '/assets/images/placeholder-cake.jpg'
};
